# Design

How the system works today. A design document changes in the same pull request as what it describes.

- [`architecture.md`](architecture.md): the packages, the service they call, the rules they share, and how each is versioned, released and built by CI. Read before changing a package's boundaries, a `version.json` or a workflow.
- [`rest-api.md`](rest-api.md): the REST contract of the DAX Formatter service, as the clients use it. Read before changing a request, a response or how a client calls the service.
