-- Proposed schema only. Run manually after the team approves it.
create table public.articles (
    id integer generated always as identity primary key,
    title text not null check (length(trim(title)) > 0),
    body text not null check (length(trim(body)) > 0),
    author text not null,
    date_published timestamptz not null default now()
);

-- Deny client access until the team defines student read/admin write policies.
alter table public.articles enable row level security;
