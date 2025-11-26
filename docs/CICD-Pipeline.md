# CI/CD Pipeline Documentation

## Overview

The EcomShopping platform uses GitHub Actions for continuous integration and continuous deployment (CI/CD). This automated pipeline ensures code quality, runs tests, builds Docker images, and prepares artifacts for deployment.

## Pipeline Architecture

### Workflow File Location

`.github/workflows/ci.yml`

### Trigger Events

The pipeline is triggered by:

1. **Push Events**
   - Branches: `main`, `develop`
   - Automatically runs on every commit to these branches

2. **Pull Request Events**
   - Targets: `main`, `develop`
   - Runs on PR creation and updates
   - Must pass before merge is allowed

## Pipeline Stages

### Stage 1: Build and Test

**Runner:** `ubuntu-latest`

**Steps:**

1. **Checkout Code**
   ```yaml
   - uses: actions/checkout@v3
   ```
   - Fetches the repository code
   - Includes submodules if present
   - Sets up git configuration

2. **Setup .NET**
   ```yaml
   - uses: actions/setup-dotnet@v3
     with:
       dotnet-version: '8.0.x'
   ```
   - Installs .NET 8.0 SDK
   - Uses latest patch version (8.0.x)
   - Caches NuGet packages for faster builds

3. **Restore Dependencies**
   ```yaml
   - run: dotnet restore
   ```
   - Downloads NuGet packages
   - Restores project dependencies
   - Uses package cache for performance

4. **Build Solution**
   ```yaml
   - run: dotnet build --no-restore --configuration Release
   ```
   - Compiles all projects in Release mode
   - Skips restore (already done)
   - Generates optimized binaries
   - Fails if compilation errors exist

5. **Run Tests**
   ```yaml
   - run: dotnet test --no-build --configuration Release --verbosity normal
   ```
   - Executes all unit and integration tests
   - Uses Release build artifacts
   - Outputs test results
   - Fails pipeline if any test fails

6. **Publish API**
   ```yaml
   - run: dotnet publish src/EcomShopping.API/EcomShopping.API.csproj 
       --configuration Release 
       --output ./publish/api
   ```
   - Creates deployment-ready API package
   - Self-contained or framework-dependent
   - Includes all dependencies
   - Optimized for production

7. **Publish Web**
   ```yaml
   - run: dotnet publish src/EcomShopping.Web/EcomShopping.Web.csproj 
       --configuration Release 
       --output ./publish/web
   ```
   - Creates deployment-ready Blazor package
   - Includes static assets
   - Optimized for production

8. **Upload API Artifacts**
   ```yaml
   - uses: actions/upload-artifact@v3
     with:
       name: api-artifacts
       path: ./publish/api
   ```
   - Stores build artifacts in GitHub
   - Available for download
   - Retained for 90 days (default)
   - Used for manual deployment

9. **Upload Web Artifacts**
   ```yaml
   - uses: actions/upload-artifact@v3
     with:
       name: web-artifacts
       path: ./publish/web
   ```
   - Stores Blazor build artifacts
   - Available for download
   - Can be deployed to hosting service

### Stage 2: Docker Build and Push

**Runner:** `ubuntu-latest`

**Conditions:**
- Only runs on `main` branch
- Requires successful build job
- Can be configured to run on tags

**Steps:**

1. **Checkout Code**
   ```yaml
   - uses: actions/checkout@v3
   ```
   - Fresh checkout for Docker build

2. **Build Docker Images**
   ```yaml
   - run: docker-compose build
   ```
   - Builds API and Web Docker images
   - Uses multi-stage Dockerfiles
   - Leverages build cache
   - Tags images appropriately

3. **Login to Docker Hub** (Optional)
   ```yaml
   - if: ${{ secrets.DOCKER_USERNAME && secrets.DOCKER_PASSWORD }}
     run: echo ${{ secrets.DOCKER_PASSWORD }} | 
          docker login -u ${{ secrets.DOCKER_USERNAME }} --password-stdin
   ```
   - Conditional step (requires secrets)
   - Authenticates with Docker Hub
   - Enables image push

4. **Push Docker Images** (Optional)
   ```yaml
   - if: ${{ secrets.DOCKER_USERNAME && secrets.DOCKER_PASSWORD }}
     run: docker-compose push
   ```
   - Pushes images to Docker Hub
   - Only if credentials configured
   - Makes images available for deployment

## Pipeline Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                     GitHub Event Trigger                     │
│              (Push to main/develop or PR)                    │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                    Build & Test Job                          │
│  ┌────────────────────────────────────────────────────┐    │
│  │ 1. Checkout Code                                    │    │
│  │ 2. Setup .NET 8.0                                   │    │
│  │ 3. Restore Dependencies                             │    │
│  │ 4. Build Solution (Release)                         │    │
│  │ 5. Run All Tests                                    │    │
│  │ 6. Publish API                                      │    │
│  │ 7. Publish Web                                      │    │
│  │ 8. Upload API Artifacts                             │    │
│  │ 9. Upload Web Artifacts                             │    │
│  └────────────────────────────────────────────────────┘    │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ▼
                  ┌───────────────┐
                  │  Success?     │
                  └───────┬───────┘
                          │ Yes (main branch only)
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                      Docker Job                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │ 1. Checkout Code                                    │    │
│  │ 2. Build Docker Images                              │    │
│  │ 3. Login to Docker Hub (if configured)              │    │
│  │ 4. Push Images (if configured)                      │    │
│  └────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

## Configuration

### Required Secrets

Configure in GitHub repository settings: `Settings → Secrets and variables → Actions`

**Optional Secrets (for Docker Hub):**

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `DOCKER_USERNAME` | Docker Hub username | `mycompany` |
| `DOCKER_PASSWORD` | Docker Hub password or token | `dckr_pat_xxxxx` |

### Environment Variables

Can be configured in workflow file or repository settings:

```yaml
env:
  DOTNET_VERSION: '8.0.x'
  BUILD_CONFIGURATION: 'Release'
  REGISTRY: 'docker.io'
```

## Local Simulation

### Running Pipeline Steps Locally

You can run equivalent commands locally to test before pushing:

```bash
# 1. Restore
dotnet restore

# 2. Build
dotnet build --configuration Release

# 3. Test
dotnet test --configuration Release --verbosity normal

# 4. Publish API
dotnet publish src/EcomShopping.API/EcomShopping.API.csproj \
  --configuration Release \
  --output ./publish/api

# 5. Publish Web
dotnet publish src/EcomShopping.Web/EcomShopping.Web.csproj \
  --configuration Release \
  --output ./publish/web

# 6. Build Docker images
docker-compose build
```

### Using Act (GitHub Actions locally)

Install Act to run GitHub Actions workflows locally:

```bash
# Install Act (macOS)
brew install act

# Install Act (Linux)
curl https://raw.githubusercontent.com/nektos/act/master/install.sh | sudo bash

# Run the workflow
act push

# Run specific job
act -j build

# Run with secrets
act -s DOCKER_USERNAME=myuser -s DOCKER_PASSWORD=mypass
```

## Deployment Strategies

### Manual Deployment

After successful pipeline:

1. Download artifacts from GitHub Actions
2. Extract to deployment location
3. Configure environment-specific settings
4. Start application

### Automated Deployment

**Azure App Service:**

Add deployment step to workflow:

```yaml
- name: Deploy to Azure Web App
  uses: azure/webapps-deploy@v2
  with:
    app-name: 'ecomshopping-api'
    publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
    package: ./publish/api
```

**Azure Container Instances:**

```yaml
- name: Deploy to Azure Container Instances
  uses: azure/aci-deploy@v1
  with:
    resource-group: EcomShopping-RG
    dns-name-label: ecomshopping-api
    image: ${{ secrets.DOCKER_USERNAME }}/ecomshopping-api:latest
    registry-username: ${{ secrets.DOCKER_USERNAME }}
    registry-password: ${{ secrets.DOCKER_PASSWORD }}
```

**Kubernetes:**

```yaml
- name: Deploy to Kubernetes
  uses: azure/k8s-deploy@v1
  with:
    manifests: |
      kubernetes/deployment.yaml
      kubernetes/service.yaml
    images: |
      ${{ secrets.DOCKER_USERNAME }}/ecomshopping-api:${{ github.sha }}
```

### GitOps Approach

For production environments, consider:

1. **ArgoCD**: Declarative GitOps for Kubernetes
2. **Flux**: GitOps toolkit for Kubernetes
3. **Separate Deployment Repository**: Store deployment configurations separately

## Branch Strategy

### Main Branch
- Protected branch
- Requires PR approval
- Runs full CI/CD pipeline
- Deploys to production (when configured)

### Develop Branch
- Integration branch for features
- Runs full CI/CD pipeline
- Deploys to staging environment

### Feature Branches
- Created from develop
- PR to develop triggers pipeline
- Must pass all checks before merge

### Release Branches
- Created from develop
- Final testing before production
- Merged to main and back to develop

## Pipeline Optimization

### Caching Strategies

**NuGet Package Caching:**

```yaml
- uses: actions/cache@v3
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
    restore-keys: |
      ${{ runner.os }}-nuget-
```

**Docker Layer Caching:**

```yaml
- uses: docker/build-push-action@v4
  with:
    cache-from: type=gha
    cache-to: type=gha,mode=max
```

### Parallel Jobs

For faster pipelines, split into parallel jobs:

```yaml
jobs:
  test-api:
    runs-on: ubuntu-latest
    steps:
      - run: dotnet test src/EcomShopping.API.Tests
  
  test-web:
    runs-on: ubuntu-latest
    steps:
      - run: dotnet test src/EcomShopping.Web.Tests
```

## Monitoring and Notifications

### Status Badges

Add to README.md:

```markdown
![Build Status](https://github.com/sorenbj/EcomShopping/workflows/CI/CD%20Pipeline/badge.svg)
```

### Slack Notifications

Add notification step:

```yaml
- name: Slack Notification
  uses: 8398a7/action-slack@v3
  with:
    status: ${{ job.status }}
    text: 'Build ${{ job.status }}'
    webhook_url: ${{ secrets.SLACK_WEBHOOK }}
  if: always()
```

### Email Notifications

Configure in GitHub settings:
- Settings → Notifications → Actions
- Email on workflow failure

## Security Best Practices

### Secrets Management

1. **Never commit secrets** to repository
2. **Use GitHub Secrets** for sensitive data
3. **Rotate credentials** regularly
4. **Limit secret access** to necessary jobs

### Dependency Scanning

Add security scanning:

```yaml
- name: Run security scan
  uses: github/codeql-action/analyze@v2
```

### Container Scanning

Scan Docker images for vulnerabilities:

```yaml
- name: Run Trivy vulnerability scanner
  uses: aquasecurity/trivy-action@master
  with:
    image-ref: 'ecomshopping-api:latest'
    format: 'sarif'
    output: 'trivy-results.sarif'
```

## Troubleshooting

### Build Failures

**Issue: Restore fails**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear
```

**Issue: Tests fail in CI but pass locally**
- Check for environment-specific dependencies
- Verify test database configuration
- Ensure tests are isolated and deterministic

**Issue: Docker build fails**
```bash
# Build locally with verbose output
docker-compose build --no-cache --progress=plain
```

### Deployment Failures

**Issue: Artifacts not found**
- Check artifact upload step completed
- Verify artifact name matches download step

**Issue: Docker push fails**
- Verify Docker Hub credentials
- Check image tag format
- Ensure repository exists

## Pipeline Metrics

### Key Performance Indicators

- **Build Time**: Target < 5 minutes
- **Test Execution**: Target < 2 minutes
- **Success Rate**: Target > 95%
- **Deployment Frequency**: Daily or per feature

### Monitoring Dashboard

Track pipeline health:
- GitHub Actions insights
- Custom dashboard (Grafana)
- Build trends over time

## Future Enhancements

### Planned Additions

1. **Code Coverage Reports**: Integrate Coverlet and report to Codecov
2. **Performance Testing**: Add load tests to pipeline
3. **Database Migration Validation**: Test migrations in pipeline
4. **Multi-environment Deployment**: Staging → Production workflow
5. **Release Automation**: Semantic versioning and changelog generation
6. **Rollback Capability**: Automatic rollback on deployment failure

### Advanced Features

```yaml
# Code coverage
- name: Code Coverage
  run: |
    dotnet test --collect:"XPlat Code Coverage"
    
- name: Upload coverage to Codecov
  uses: codecov/codecov-action@v3

# Performance testing
- name: Run load tests
  run: |
    dotnet run --project tests/LoadTests

# Database migrations
- name: Test migrations
  run: |
    dotnet ef database update --project src/EcomShopping.Infrastructure
```

## Best Practices

1. **Keep pipelines fast**: Target under 10 minutes
2. **Fail fast**: Run quick checks first
3. **Cache dependencies**: Reduce build time
4. **Run tests in parallel**: Speed up test execution
5. **Use matrix builds**: Test multiple configurations
6. **Monitor pipeline health**: Track success rates and duration
7. **Regular maintenance**: Update actions and dependencies
8. **Document changes**: Update this document when pipeline changes

## Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Docker Documentation](https://docs.docker.com/)
- [.NET CLI Reference](https://docs.microsoft.com/en-us/dotnet/core/tools/)
- [Azure DevOps](https://azure.microsoft.com/en-us/services/devops/)

## Conclusion

The CI/CD pipeline ensures:
- **Quality**: Automated testing catches issues early
- **Consistency**: Same process for every change
- **Speed**: Fast feedback on code changes
- **Reliability**: Repeatable deployment process
- **Confidence**: Tested code reaches production

This automated approach supports rapid, reliable software delivery while maintaining high quality standards.
