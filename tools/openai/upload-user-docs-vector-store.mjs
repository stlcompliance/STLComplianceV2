#!/usr/bin/env node
import { createHash } from "node:crypto";
import { mkdir, readdir, readFile, writeFile } from "node:fs/promises";
import path from "node:path";

const API_BASE = "https://api.openai.com/v1";
const DEFAULT_DOCS_ROOT = "docs/user";
const DEFAULT_MANIFEST = ".tmp-openai-user-docs-vector-store.json";

const args = parseArgs(process.argv.slice(2));
const apiKey = process.env.OPENAI_API_KEY;
if (!apiKey) {
  fail("OPENAI_API_KEY is required. Set it in the current shell before running this script.");
}

const repoRoot = process.cwd();
const docsRoot = path.resolve(repoRoot, args.docsRoot ?? DEFAULT_DOCS_ROOT);
const manifestPath = path.resolve(repoRoot, args.manifest ?? DEFAULT_MANIFEST);
const vectorStoreName =
  args.name ?? `STLComplianceV2 user docs ${new Date().toISOString().slice(0, 10)}`;

const files = await listMarkdownFiles(docsRoot);
if (files.length === 0) {
  fail(`No markdown files found under ${docsRoot}`);
}

console.log(`Found ${files.length} user documentation files.`);

const vectorStoreId =
  args.vectorStoreId ?? (await createVectorStore(vectorStoreName)).id;
console.log(`Using vector store ${vectorStoreId}.`);

const uploaded = [];
for (const filePath of files) {
  const relativePath = normalizeRelativePath(path.relative(docsRoot, filePath));
  const bytes = await readFile(filePath);
  const sha256 = createHash("sha256").update(bytes).digest("hex");
  const fileName = `docs_user__${relativePath.replace(/[\\/]/g, "__")}`;
  const uploadedFile = await uploadFile(fileName, bytes);
  const attributes = buildAttributes(relativePath, sha256);
  const vectorFile = await attachFileToVectorStore(vectorStoreId, uploadedFile.id, attributes);

  uploaded.push({
    path: relativePath,
    sha256,
    openAiFileId: uploadedFile.id,
    vectorStoreFileStatus: vectorFile.status ?? "unknown",
    attributes,
  });
  console.log(`Uploaded ${relativePath} -> ${uploadedFile.id}`);
}

const indexed = await waitForVectorStoreFiles(vectorStoreId, uploaded);
const manifest = {
  generatedAt: new Date().toISOString(),
  vectorStoreId,
  vectorStoreName,
  docsRoot,
  fileCount: uploaded.length,
  files: indexed,
};

await mkdir(path.dirname(manifestPath), { recursive: true });
await writeFile(manifestPath, `${JSON.stringify(manifest, null, 2)}\n`, "utf8");

console.log("");
console.log(`Indexed ${indexed.filter((file) => file.vectorStoreFileStatus === "completed").length}/${indexed.length} files.`);
console.log(`Manifest written to ${manifestPath}`);
console.log("");
console.log("Configure the assistant with:");
console.log(`OPENAI_ASSISTANT_VECTOR_STORE_IDS=${vectorStoreId}`);

function parseArgs(values) {
  const parsed = {};
  for (let i = 0; i < values.length; i += 1) {
    const value = values[i];
    if (!value.startsWith("--")) continue;
    const key = value.slice(2).replace(/-([a-z])/g, (_, letter) => letter.toUpperCase());
    const next = values[i + 1];
    if (!next || next.startsWith("--")) {
      parsed[key] = true;
    } else {
      parsed[key] = next;
      i += 1;
    }
  }
  return parsed;
}

async function listMarkdownFiles(root) {
  const found = [];
  await walk(root, found);
  return found.sort((a, b) => a.localeCompare(b));
}

async function walk(directory, found) {
  const entries = await readdir(directory, { withFileTypes: true });
  for (const entry of entries) {
    const fullPath = path.join(directory, entry.name);
    if (entry.isDirectory()) {
      await walk(fullPath, found);
    } else if (entry.isFile() && entry.name.toLowerCase().endsWith(".md")) {
      found.push(fullPath);
    }
  }
}

async function createVectorStore(name) {
  return requestJson("/vector_stores", {
    method: "POST",
    body: {
      name,
      metadata: {
        source: "STLComplianceV2",
        corpus: "docs/user",
      },
    },
  });
}

async function uploadFile(fileName, bytes) {
  const form = new FormData();
  form.set("purpose", "assistants");
  form.set("file", new Blob([bytes], { type: "text/markdown" }), fileName);
  return requestJson("/files", {
    method: "POST",
    form,
  });
}

async function attachFileToVectorStore(vectorStoreId, fileId, attributes) {
  try {
    return await requestJson(`/vector_stores/${encodeURIComponent(vectorStoreId)}/files`, {
      method: "POST",
      body: {
        file_id: fileId,
        attributes,
      },
    });
  } catch (error) {
    if (error.status !== 400) {
      throw error;
    }

    const created = await requestJson(`/vector_stores/${encodeURIComponent(vectorStoreId)}/files`, {
      method: "POST",
      body: {
        file_id: fileId,
      },
    });
    await requestJson(
      `/vector_stores/${encodeURIComponent(vectorStoreId)}/files/${encodeURIComponent(fileId)}`,
      {
        method: "POST",
        body: { attributes },
      },
    );
    return created;
  }
}

async function waitForVectorStoreFiles(vectorStoreId, uploaded) {
  const pending = new Map(uploaded.map((file) => [file.openAiFileId, file]));
  const deadline = Date.now() + 10 * 60 * 1000;

  while (pending.size > 0 && Date.now() < deadline) {
    for (const [fileId, file] of [...pending]) {
      const status = await requestJson(
        `/vector_stores/${encodeURIComponent(vectorStoreId)}/files/${encodeURIComponent(fileId)}`,
        { method: "GET" },
      );
      file.vectorStoreFileStatus = status.status ?? "unknown";
      if (["completed", "failed", "cancelled"].includes(file.vectorStoreFileStatus)) {
        pending.delete(fileId);
      }
    }

    if (pending.size > 0) {
      console.log(`Waiting on ${pending.size} vector store files...`);
      await delay(3000);
    }
  }

  if (pending.size > 0) {
    console.warn(`Timed out while waiting on ${pending.size} vector store files.`);
  }

  return uploaded;
}

function buildAttributes(relativePath, sha256) {
  const parts = relativePath.split("/");
  return {
    corpus: "docs/user",
    source_path: `docs/user/${relativePath}`,
    section: parts[0] ?? "root",
    product: inferProduct(relativePath),
    sha256,
  };
}

function inferProduct(relativePath) {
  const lower = relativePath.toLowerCase();
  for (const product of [
    "nexarr",
    "staffarr",
    "trainarr",
    "maintainarr",
    "routarr",
    "supplyarr",
    "loadarr",
    "compliance-core",
    "field-companion",
    "reportarr",
    "recordarr",
  ]) {
    if (lower.includes(product)) {
      return product.replace("-", "");
    }
  }

  return "suite";
}

async function requestJson(endpoint, options) {
  const headers = {
    Authorization: `Bearer ${apiKey}`,
  };
  const init = {
    method: options.method,
    headers,
  };

  if (options.form) {
    init.body = options.form;
  } else if (options.body) {
    headers["Content-Type"] = "application/json";
    init.body = JSON.stringify(options.body);
  }

  const response = await fetch(`${API_BASE}${endpoint}`, init);
  const text = await response.text();
  if (!response.ok) {
    const message = extractErrorMessage(text) ?? `${response.status} ${response.statusText}`;
    const error = new Error(`OpenAI request failed for ${endpoint}: ${message}`);
    error.status = response.status;
    error.body = text;
    throw error;
  }

  return text ? JSON.parse(text) : {};
}

function extractErrorMessage(text) {
  if (!text) return null;
  try {
    const parsed = JSON.parse(text);
    return parsed?.error?.message ?? parsed?.message ?? null;
  } catch {
    return text;
  }
}

function normalizeRelativePath(value) {
  return value.split(path.sep).join("/");
}

function delay(milliseconds) {
  return new Promise((resolve) => setTimeout(resolve, milliseconds));
}

function fail(message) {
  console.error(message);
  process.exit(1);
}
