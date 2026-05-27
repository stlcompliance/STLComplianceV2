import http from 'k6/http';
import { check } from 'k6';
import { loadDemoCredentials, normalizeBaseUrl } from './stl-config.js';

const jsonHeaders = { 'Content-Type': 'application/json' };

export function loginNexArr(nexarrBaseUrl, credentials = loadDemoCredentials()) {
  const baseUrl = normalizeBaseUrl(nexarrBaseUrl);
  const response = http.post(
    `${baseUrl}/api/auth/login`,
    JSON.stringify({
      email: credentials.email,
      password: credentials.password,
      tenantId: credentials.tenantId,
    }),
    { headers: jsonHeaders, tags: { name: 'nexarr_login' } },
  );

  check(response, {
    'nexarr login status 200': (r) => r.status === 200,
    'nexarr login returns accessToken': (r) => {
      try {
        return Boolean(r.json('accessToken'));
      } catch {
        return false;
      }
    },
  });

  if (response.status !== 200) {
    return null;
  }

  return response.json('accessToken');
}

export function getAuthorizedMe(baseUrl, accessToken, productKey) {
  const response = http.get(`${normalizeBaseUrl(baseUrl)}/api/me`, {
    headers: { Authorization: `Bearer ${accessToken}` },
    tags: { name: `${productKey}_me` },
  });

  check(response, {
    [`${productKey} /api/me status 200`]: (r) => r.status === 200,
    [`${productKey} /api/me has email`]: (r) => {
      try {
        return Boolean(r.json('email'));
      } catch {
        return false;
      }
    },
  });

  return response;
}

export function createHandoff(nexarrBaseUrl, accessToken, productKey, callbackUrl = null) {
  const response = http.post(
    `${normalizeBaseUrl(nexarrBaseUrl)}/api/launch/handoff`,
    JSON.stringify({ productKey, callbackUrl }),
    {
      headers: {
        ...jsonHeaders,
        Authorization: `Bearer ${accessToken}`,
      },
      tags: { name: `${productKey}_handoff` },
    },
  );

  check(response, {
    [`${productKey} handoff status 200`]: (r) => r.status === 200,
    [`${productKey} handoff returns code`]: (r) => {
      try {
        return Boolean(r.json('handoffCode'));
      } catch {
        return false;
      }
    },
  });

  if (response.status !== 200) {
    return null;
  }

  return response.json('handoffCode');
}

export function redeemHandoff(productBaseUrl, handoffCode, productKey) {
  const response = http.post(
    `${normalizeBaseUrl(productBaseUrl)}/api/auth/handoff/redeem`,
    JSON.stringify({ handoffCode }),
    { headers: jsonHeaders, tags: { name: `${productKey}_redeem` } },
  );

  check(response, {
    [`${productKey} redeem status 200`]: (r) => r.status === 200,
    [`${productKey} redeem returns accessToken`]: (r) => {
      try {
        return Boolean(r.json('accessToken'));
      } catch {
        return false;
      }
    },
  });

  if (response.status !== 200) {
    return null;
  }

  return response.json('accessToken');
}

export function bootstrapProductSession(nexarrBaseUrl, productBaseUrl, productKey, credentials) {
  const nexarrToken = loginNexArr(nexarrBaseUrl, credentials);
  if (!nexarrToken) {
    return null;
  }

  const handoffCode = createHandoff(nexarrBaseUrl, nexarrToken, productKey);
  if (!handoffCode) {
    return null;
  }

  return redeemHandoff(productBaseUrl, handoffCode, productKey);
}
