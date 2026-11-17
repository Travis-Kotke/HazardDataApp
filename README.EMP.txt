Eric Puksich

My main responsibility was the Home page for HazardDataApp. I used AI to help
implement a report table based on our team's storyboard, with incident titles,
locations, dates, severity, publication status, and resolution status. The page
includes filters for incident title, date range, severity, public reports, and
resolved status, along with Refresh and Clear filters controls. I also worked
with the AI to connect the homepage to our existing Supabase reports table
through the BusinessLogic and Database layers.

I supervised the AI by providing the storyboard, explaining what the controls
should mean, and gave it the names used in the database. I reviewed the running
Windows app and requested changes when the layout did not match what I wanted.
These included removing repeated Home headings, keeping the app title above the
tabs, aligning the filter controls, replacing awkward checkboxes with switches,
and making the flag button a smaller square icon so the resolution status was
easier to read.

I asked the AI to build the project and check its work instead of only generating
code. The AI reported successful Android and Windows builds and ran targeted
checks for report filtering and service integration. When our teammates merged
their screens, I requested a local merge of the latest main before publishing
our branch. This revealed conflicts and a demo-service registration that needed
to remain separate from the homepage's real report service.

One problem was that Supabase returned seven reports, but the homepage displayed
blank rows. After several attempts, the AI identified a XAML namespace error in
the row template. Moving the namespace declaration to the page root fixed it,
and the AI verified the displayed rows and title search in the running app. This
showed me that a successful build or database request does not prove the screen
works correctly; it also needs to be checked while running.
