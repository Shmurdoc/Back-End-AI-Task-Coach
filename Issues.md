# Issues & Limitations

Here’s a running list of what’s not perfect yet, what I know needs work, and what I’ve already fixed. I keep this honest so you know exactly where things stand.

## Known Issues
- Some repository and integration tests are still failing (see test logs)
- Authentication/authorization (JWT/OAuth2) isn’t done yet
- Task and goal management logic is still in progress
- No AI/ML integration for advanced coaching (yet!)
- No production-grade secret management (like Azure Key Vault)
- No frontend UI—this is backend/API only for now

## Technical Debt
- CQRS split could be stricter in a few places
- Role-based policies (Coach, Admin, User) need more work
- Some DTOs and service interfaces are just stubs
- Test coverage for edge cases and error handling could be better

## What I Want to Improve
- Add user authentication and authorization
- Finish task/goal management features
- Integrate AI/ML for smarter coaching
- Add production-grade secret management
- Expand docs with more code samples and onboarding tips
- Make error handling and validation even more robust

## Recent Fixes
- Locked down config and secrets management
- Upgraded metrics and observability
- Docker and CI/CD pipeline are now production-ready
- Real-time notifications and Redis caching are in place

## Want to Help?
- Check open issues here or on GitHub
- PRs with clear descriptions and tests are always welcome
- Got ideas? Propose them in GitHub Discussions
