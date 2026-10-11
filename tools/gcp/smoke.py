import argparse
from concurrent.futures import ThreadPoolExecutor
import http.cookiejar
import json
from pathlib import Path
import urllib.error
import urllib.request

from deploy import FRONTENDS, PRODUCTS, WORKERS, frontend_api, gcloud


def request_json(opener, url, data=None, headers=None):
    request = urllib.request.Request(url, data=None if data is None else json.dumps(data).encode(),
                                     headers={"Content-Type": "application/json", **(headers or {})})
    with opener.open(request, timeout=90) as response:
        if response.status == 204:
            return None
        if "application/json" not in response.headers.get("Content-Type", ""):
            raise RuntimeError("Expected JSON, not an HTML fallback.")
        return json.load(response)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--project", required=True)
    parser.add_argument("--region", default="us-central1")
    parser.add_argument("--config", required=True)
    parser.add_argument("--tag", required=True)
    options = parser.parse_args()
    config = json.loads(Path(options.config).read_text(encoding="utf-8"))
    services = json.loads(gcloud("run", "services", "list", "--project", options.project,
                                 "--region", options.region, "--format=json"))
    inventory = {service["metadata"]["name"]: service for service in services}
    identity_token = gcloud("auth", "print-identity-token")
    expected = [product.lower() + "-api" for product in PRODUCTS] + [
        product.lower() + "-worker" for product in WORKERS] + FRONTENDS

    def probe(name):
        try:
            service = inventory[name]
            if not any(condition.get("type") == "Ready" and condition.get("status") == "True"
                       for condition in service.get("status", {}).get("conditions", [])):
                raise RuntimeError("Cloud Run service is not Ready.")
            image = service["spec"]["template"]["spec"]["containers"][0]["image"]
            if not image.endswith(":" + options.tag):
                raise RuntimeError("Service is not on the requested release image.")
            origin = service["status"]["url"]
            path = "/api/v1/health/ready" if name in FRONTENDS else "/health/ready"
            headers = {}
            if name.endswith("-worker"):
                path = "/health"
                headers["Authorization"] = "Bearer " + identity_token
            opener = urllib.request.build_opener()
            result = request_json(opener, origin + path, headers=headers)
            if result.get("status", "").lower() != "healthy":
                raise RuntimeError("Readiness response is not healthy.")
            if name in FRONTENDS and result.get("product") != frontend_api(name).removesuffix("-api"):
                raise RuntimeError("Frontend routes to the wrong owning API.")
            return {"service": name, "passed": True}
        except Exception as error:
            return {"service": name, "passed": False, "error": str(error)}

    with ThreadPoolExecutor(max_workers=6) as pool:
        results = list(pool.map(probe, expected))
    print(json.dumps({"serviceChecks": results}, indent=2), flush=True)
    if any(not result["passed"] for result in results):
        raise SystemExit(1)

    suite = inventory["suite-frontend"]["status"]["url"]
    jar = http.cookiejar.CookieJar()
    opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(jar))
    secret, version = config["nexarrSecrets"]["Seed__FirstAdminPassword"].split(":")
    password = gcloud("secrets", "versions", "access", version, "--secret", secret, "--project", options.project)
    session = request_json(opener, suite + "/api/v1/auth/login", {
        "email": config["serviceEnvironment"]["nexarr-api"]["Seed__FirstAdminEmail"],
        "password": password,
    }, {"X-Stl-Cookie-Session": "true"})
    headers = {"Authorization": "Bearer " + session["accessToken"], "X-Stl-Cookie-Session": "true"}
    try:
        if session.get("refreshToken"):
            raise RuntimeError("Cookie-session login exposed a refresh token in the response body.")
        if not any(cookie.name == "stl.refresh_token" and cookie.secure for cookie in jar):
            raise RuntimeError("Secure browser refresh cookie was not set.")
        for product in [name.removesuffix("-frontend") for name in FRONTENDS
                        if name.endswith("-frontend") and name != "suite-frontend"]:
            result = request_json(opener, suite + "/api/v1/launch/validate", {"productKey": product}, headers)
            if not result.get("canLaunch") or not result.get("launchUrl", "").startswith("https://"):
                raise RuntimeError(f"Launch validation failed for {product}: {result.get('reasonCode')}")
        renewed = request_json(opener, suite + "/api/v1/auth/refresh", {"refreshToken": ""}, headers)
        if not renewed.get("accessToken"):
            raise RuntimeError("Browser session renewal failed.")
        print("Browser-cookie login, session renewal and all product launch validations passed.")
    finally:
        request_json(opener, suite + "/api/v1/auth/logout", {"refreshToken": ""}, headers)


if __name__ == "__main__":
    main()
