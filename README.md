#A HAR file investigation tool to troubleshoot HAR file traces from browser tools

# HAR File Investigator

`HAR File Investigator` is a Windows desktop tool for loading, searching, filtering, tagging, and exporting HTTP traffic from HAR files.

## Features

- Open `.har` / `.json` HAR traces
- Grid view of requests with key columns (`Started`, `Method`, `Host`, `URL`, `Status`, `MIME`, `Time`, `Tags`)
- Dynamic/extended columns from HAR metadata and headers
- Query language with:
  - text terms
  - field filters (for example `status=200`, `method=GET`, `ReqHeader.content-type:json`)
  - operators: `=`, `!=`, `:`, `<`, `<=`, `>`, `>=`
  - boolean terms: `and`, `or`, and negation with `!`
- Highlighted matches with next/previous navigation
- Match highlighting inside request/response panes (Raw + JWT tabs)
- Row tagging and tag management
- Delete selected rows from the current view (`Delete` key or row context menu)
- `Clear Sessions` button to remove all loaded entries
- Timeline view for highlighted rows
- Export filtered rows to CSV
- Save HAR file with tags
- Light/Dark theme with persisted UI settings

## How to use

1. Click `Open HAR...` and select a HAR file.
2. Enter a query in the `Query` box.
3. Click `Apply` (or press `Enter`) to run the query.
4. Toggle `Filter (On/Off)` to control whether non-matching rows are hidden.
5. Use `<` and `>` buttons to move through highlighted matches.
6. Select rows and click `Tag` (or right-click for tag options).
7. Press `Delete` to remove selected rows from the current session view.
8. Click `Clear Sessions` to clear all loaded rows.
9. Use `Export CSV...` to export filtered rows.
10. Use `File` -> `Save HAR File...` to save tags back into a HAR file copy.

## Query examples

- `status=200 and method=GET`
- `host:login.microsoftonline.com`
- `ReqHeader.authorization:Bearer`
- `RespHeader.content-type:json`
- `Session.pageref=page_1`
- `!status=200 and response:"error"`
