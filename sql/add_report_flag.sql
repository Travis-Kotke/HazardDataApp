-- Run manually before using the homepage's flag action.
alter table public.reports
    add column if not exists is_flagged boolean not null default false;

-- Keep existing RLS enabled. The team's admin UPDATE policy must authorize
-- flag changes; never grant general anonymous write access to reports.
