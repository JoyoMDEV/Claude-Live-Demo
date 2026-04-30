# my-example-project

> .NET 9 WebAPI + Angular 20 Monorepo

## Getting Started

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [VS Code](https://code.visualstudio.com/) + [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)

### Option A — Dev Container (recommended)
Open the repo in VS Code and select **"Reopen in Container"** when prompted.
All tools, extensions, and hooks will be installed automatically.

### Option B — Local setup
```bash
# 1. Restore .NET local tools (Husky.Net + CSharpier)
dotnet tool restore

# 2. Activate git hooks
dotnet husky install

# 3. Install frontend dependencies + register hooks
cd frontend && npm install
```

### Project structure
| Path | Description |
|------|-------------|
| `backend/` | .NET 9 WebAPI |
| `frontend/` | Angular 20+ SPA |
| `.devcontainer/` | Dev Container config |
| `.github/workflows/` | CI pipelines |
| `.husky/` | Git hook scripts |

## CI Workflows
| Workflow | Trigger |
|----------|---------|
| [ci-backend.yml](.github/workflows/ci-backend.yml) | Push/PR to `main` touching `backend/` |
| [ci-frontend.yml](.github/workflows/ci-frontend.yml) | Push/PR to `main` touching `frontend/` |
| [pr-checks.yml](.github/workflows/pr-checks.yml) | All PRs — gate job |

> 💡 Set `all-checks-passed` as the required status check in **GitHub Settings → Branches → Branch protection rules**.
