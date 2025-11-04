# GitHub Actions Workflows

This directory contains the CI/CD workflows for TiroTime.

## Workflows

### CI (`ci.yml`)

**Triggers:**
- Push to `main` branch
- Pull requests to `main` branch

**Steps:**
1. Checkout code
2. Setup .NET 9.0
3. Restore NuGet packages
4. Build solution (Release configuration)
5. Run all unit tests
6. Upload test results as artifacts

**Status Badge:**
```markdown
[![CI](https://github.com/abierhaus/TiroTime/actions/workflows/ci.yml/badge.svg)](https://github.com/abierhaus/TiroTime/actions/workflows/ci.yml)
```

### Docker Build (`docker.yml`)

**Triggers:**
- Push to `main` branch
- Push of version tags (`v*.*.*`)
- Pull requests to `main` branch

**Steps:**
1. Checkout code
2. Setup Docker Buildx
3. Build Docker image
4. Validate image was created

**Features:**
- Layer caching for faster builds
- Multi-stage build optimization
- No push to registry (local validation only)

## Running Locally

You can test the workflows locally using [act](https://github.com/nektos/act):

```bash
# Test CI workflow
act push

# Test specific workflow
act -W .github/workflows/ci.yml

# Test with specific event
act pull_request
```

## Artifacts

Test results are uploaded as artifacts and available for 90 days:
- Navigate to Actions → Select workflow run → Artifacts
- Download `test-results.zip`
- Extract and view `.trx` files in Visual Studio or Test Explorer

## Secrets Required

Currently, no secrets are required as the workflows only perform builds and tests.

## Future Enhancements

Potential additions:
- Code coverage reports
- Security scanning
- Automated releases
- Docker Hub publishing
- NuGet package publishing
