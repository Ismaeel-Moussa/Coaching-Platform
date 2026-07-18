# Release process

## Pull requests

Every change is developed on a branch and merged through a pull request. The `main` branch must require these checks:

- `Backend / Test and build`
- `Frontend / Test and build`

Require the branch to be up to date before merging, block direct pushes, and do not allow force pushes or deletion. Do not require `Production / Render` on pull requests because that job only runs after a successful merge to `main`.

## Continuous integration

The CI/CD workflow runs for every pull request to `main` and every push to `main`.

The backend gate restores and builds the .NET solution in Release mode, runs all backend tests, and builds the same Dockerfile used by Render. The frontend gate installs the locked npm dependencies, runs Vitest, and creates the TypeScript/Vite production build.

Frontend tests are part of normal development. Each feature and bug fix should add or update the tests that protect its behavior. Do not defer testing until the end of the project.

## Production deployment

Set the Render service's **Auto-Deploy** setting to **Off**. GitHub Actions is the only production release controller.

After both CI gates pass on `main`, GitHub Actions:

1. verifies Render automatic deployments are disabled;
2. records the current live deployment as the rollback target;
3. asks Render to build and deploy the exact tested Git commit;
4. waits for that deployment to become live;
5. checks `/api/health` and confirms both the API and Supabase connection are healthy.

If the Render build fails, Render leaves the previous production instance active. If the new release becomes live but fails health checks, the workflow rolls back to the recorded healthy deployment and verifies health again. If deployment monitoring fails, the workflow cancels the unverified deploy or rolls it back if it already became live.

Vercel remains connected to GitHub. Required pull-request checks prevent an untested frontend commit from reaching `main`; Vercel keeps the previous production deployment active when a new build fails.

## Database safety

Render rollback restores application code and its build artifact. It does **not** roll back Supabase schema or data. Production migrations must use the expand/contract pattern:

1. add backward-compatible schema first;
2. deploy code that works with both old and new schema;
3. migrate/backfill data separately;
4. remove obsolete schema only in a later release after the rollback window closes.

Never automate destructive down-migrations against production.
