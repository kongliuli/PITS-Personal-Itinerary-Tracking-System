# PITS personal itinerary evolution

PITS should evolve as a personal time-space ledger before it becomes any larger
platform.

## Product line

- Keep the main loop local-first: plan, record, compare, review.
- Keep `TripPlan` for intended activity and `Trip` for what actually happened.
- Keep imports in staging until the user confirms them.
- Keep AI writes behind confirmation cards.

## Near-term phases

1. Personal loop
   - Calendar shows planned and actual items together.
   - Record page can turn a plan into an actual trip.
   - Stats show completion rate, delay count, frequent places, and commute time.

2. Automatic collection
   - ICS import first.
   - Google Takeout and GPX remain historical imports.
   - Email parsing comes after the staging flow is stable.

3. Privacy and backup
   - Exports must pass through visibility-based redaction.
   - Backups stay local database files.
   - Syncthing is optional after local backup/restore is boring.

4. AI assistant
   - Rules first, local LLM later.
   - AI can suggest plans, query history, and summarize periods.
   - AI never writes without user confirmation.

## Current defaults

- No team collaboration.
- No plugin marketplace.
- No cloud account.
- No Semantic Kernel dependency in the MVP App.
