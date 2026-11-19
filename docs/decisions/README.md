# Decisions

Architectural decision records in [MADR 4.0.0](https://github.com/adr/madr/tree/4.0.0) format, one file per decision: `NNNN-title-with-dashes.md`. Each record's status is in its front matter.

- Accepted records bind the code: a change that contradicts one starts with a new record that supersedes it.
- An accepted record is not rewritten; when superseded, only its status and date change.
- Fixing a typo or a broken link is not a change of decision: it is allowed in any record and leaves its status and date as they are.
- Numbers are never reused; when two branches take the same number, the record merged second takes the next free one.
