import unittest
from unittest.mock import Mock, patch

import bootstrap


class BootstrapTests(unittest.TestCase):
    def test_failure_redacts_password(self):
        result = Mock(returncode=1, stderr="failed --password=private-value", stdout="")
        with patch.object(bootstrap.subprocess, "run", return_value=result):
            with self.assertRaises(RuntimeError) as failure:
                bootstrap.command("example", "sql", "users", "create", "user", "--password=private-value")
        self.assertNotIn("private-value", str(failure.exception))

    def test_existing_secret_is_not_rotated(self):
        value_factory = Mock()
        with patch.object(bootstrap, "command", side_effect=[
            '[{"name":"projects/example/secrets/key"}]',
            '[{"name":"projects/example/secrets/key/versions/1"},{"name":"projects/example/secrets/key/versions/3"}]',
            "",
        ]) as command:
            self.assertEqual(bootstrap.ensure_secret("example", "key", value_factory, "runtime"), "key:3")
            value_factory.assert_not_called()
            self.assertEqual(command.call_count, 3)

    def test_new_secret_uses_stdin_not_command_arguments(self):
        with patch.object(bootstrap, "command", side_effect=[
            "[]", "", "[]", '{"name":"projects/example/secrets/key/versions/1"}', "",
        ]) as command:
            self.assertEqual(bootstrap.ensure_secret("example", "key", lambda: "private-value", "runtime"), "key:1")
            creation = command.call_args_list[3]
            self.assertEqual(creation.kwargs["data"], "private-value")
            self.assertNotIn("private-value", creation.args)
            self.assertIn("--data-file=-", creation.args)


if __name__ == "__main__":
    unittest.main()
