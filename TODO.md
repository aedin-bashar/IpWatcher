# TODO

## High priority
- [x] Persist IPs in SQLite instead of a text file (Infrastructure/Storage)
- [x] Switch SQLite persistence to EF Core + Migrations
- [ ] Add logging (Worker + Infrastructure)

## Tests
- [ ] Add unit tests for use cases (IpWatcher.Application)
- [ ] Add integration tests for end-to-end flow (IpWatcher.Worker)

## Config / Ops
- [ ] Move job intervals + parameters into appsettings.json (Worker)
- [ ] Add CI pipeline (build + test)

## Docs
- [ ] Add README.md (setup, Windows Service install, configuration)