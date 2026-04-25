import argparse
import json
import os
import traceback

from mem0 import MemoryClient


def print_json(title: str, value) -> None:
    print(f"\n===== {title} =====")
    print(json.dumps(value, ensure_ascii=False, indent=2, default=str))


def run_step(name: str, fn) -> None:
    print(f"\n===== {name} =====")
    try:
        result = fn()
        print(json.dumps(result, ensure_ascii=False, indent=2, default=str))
    except Exception as exc:  # pragma: no cover - diagnostic script
        print(f"{type(exc).__name__}: {exc}")
        traceback.print_exc()


def main() -> None:
    parser = argparse.ArgumentParser(description="Smoke test the installed Mem0 Python SDK.")
    parser.add_argument(
        "--memory-id",
        default="681419bb-c2a3-49ce-8a55-68c5dc8c3d24",
        help="Memory id to fetch with client.get().",
    )
    parser.add_argument(
        "--query",
        default="autoteklasuanfa mem0 restore smoke test 2026-04-22 TopologyRewrite DefinitionClause",
        help="Query to use with client.search().",
    )
    parser.add_argument("--user-id", default="Mem0 Kevin Key")
    parser.add_argument("--agent-id", default="codex-project::autoteklasuanfa")
    parser.add_argument("--app-id", default="codex-desktop")
    args = parser.parse_args()

    api_key = os.environ["MEM0_API_KEY"]
    client = MemoryClient(api_key=api_key)

    filters = {
        "AND": [
            {"user_id": args.user_id},
            {"agent_id": args.agent_id},
            {"app_id": args.app_id},
        ]
    }

    print_json(
        "client_context",
        {
            "org_id": getattr(client, "org_id", None),
            "project_id": getattr(client, "project_id", None),
            "user_email": getattr(client, "user_email", None),
            "filters": filters,
        },
    )

    run_step("get_project", lambda: client.get_project())
    run_step("get_memory", lambda: client.get(memory_id=args.memory_id))
    run_step("get_all", lambda: client.get_all(filters=filters, version="v2"))
    run_step("search", lambda: client.search(query=args.query, filters=filters, version="v2"))


if __name__ == "__main__":
    main()
