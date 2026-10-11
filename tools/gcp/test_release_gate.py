from datetime import datetime, timezone
import unittest

import release_gate


class ReleaseGateTests(unittest.TestCase):
    def test_default_does_not_exempt_render_checks(self):
        self.assertEqual(release_gate.test_filter(), "Category!=Live")

    def test_only_two_named_render_checks_are_exempted(self):
        result = release_gate.test_filter(release_gate.EXCEPTION_ID, datetime(2026, 10, 11, tzinfo=timezone.utc))
        self.assertEqual(result.count("FullyQualifiedName!="), 2)
        for check in release_gate.EXCLUDED_CHECKS:
            self.assertIn(check, result)
        self.assertNotIn("Category!=Ci", result)

    def test_unknown_exception_is_rejected(self):
        with self.assertRaises(ValueError):
            release_gate.test_filter("skip-all")

    def test_expiration_is_enforced_at_boundary(self):
        with self.assertRaisesRegex(ValueError, "expired"):
            release_gate.test_filter(release_gate.EXCEPTION_ID, release_gate.EXPIRES_AT)
