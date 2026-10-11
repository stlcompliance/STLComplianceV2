import unittest
from unittest.mock import MagicMock

from smoke import request_json


class SmokeTests(unittest.TestCase):
    def test_html_is_not_accepted_as_api_success(self):
        opener = MagicMock()
        response = opener.open.return_value.__enter__.return_value
        response.status = 200
        response.headers = {"Content-Type": "text/html"}
        with self.assertRaisesRegex(RuntimeError, "HTML"):
            request_json(opener, "https://example.com/api/v1/health")

    def test_logout_no_content_is_accepted(self):
        opener = MagicMock()
        response = opener.open.return_value.__enter__.return_value
        response.status = 204
        self.assertIsNone(request_json(opener, "https://example.com/api/v1/auth/logout", {}))
