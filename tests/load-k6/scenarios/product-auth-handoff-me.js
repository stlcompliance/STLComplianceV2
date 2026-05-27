/**
 * M13 load test — cross-product handoff bootstrap (login → handoff → redeem → /api/me).
 * Engineering-default thresholds mirror StlLoadTestSloCatalog.ProductAuthHandoffMeKey.
 */
import { sleep } from 'k6';
import {
  apiEndpoints,
  loadDemoCredentials,
  loadScenarioOptions,
  productApiEndpoints,
} from '../lib/stl-config.js';
import {
  createHandoff,
  getAuthorizedMe,
  loginNexArr,
  redeemHandoff,
} from '../lib/stl-auth.js';

const nexarrBaseUrl = apiEndpoints.find((endpoint) => endpoint.key === 'nexarr').url;

export const options = {
  scenarios: {
    product_auth_handoff_me: loadScenarioOptions(2, '30s'),
  },
  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<8000'],
    http_reqs: ['count>=12'],
  },
};

export default function () {
  const credentials = loadDemoCredentials();
  const nexarrToken = loginNexArr(nexarrBaseUrl, credentials);
  if (!nexarrToken) {
    sleep(0.3);
    return;
  }

  for (const product of productApiEndpoints) {
    const handoffCode = createHandoff(nexarrBaseUrl, nexarrToken, product.key);
    if (!handoffCode) {
      continue;
    }

    const productToken = redeemHandoff(product.url, handoffCode, product.key);
    if (productToken) {
      getAuthorizedMe(product.url, productToken, product.key);
    }
  }

  sleep(0.4);
}
