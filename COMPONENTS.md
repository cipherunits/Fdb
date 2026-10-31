# Fdb Studio — UI Components & Architecture Documentation

## 1. Overview

**Fdb Studio** is a modern, high-performance database management desktop application (similar to DBeaver, SSMS, and VS Code) built with **.NET 10**, **Avalonia UI 12.1.1**, **FluentAvalonia 3.1.0**, and **AvaloniaEdit 12.0.0**.

The user interface follows a strictly componentized, reusable MVVM architecture with dynamic responsive layouts, bounded splitters, auto-expanding uniform tabs, and a modern clean light aesthetic.

### Iconography System
All icons across the application strictly use **Microsoft Fluent vector icons** (`FluentAvalonia.UI.Controls.FASymbolIcon`) and vector `PathIcon` geometries. All emojis, stickers, and informal glyphs have been completely eliminated from the codebase in favor of sharp, scalable vector typography.

---

## 2. Component Catalog Directory

All visual elements are organized into modular, reusable `UserControl` components in the `Fdb.Components` namespace (`Components/` folder):

```
Components/
├── Layout & Workspace Components:
│   ├── NavbarComponent.axaml               # Top navigation bar & native menu system
│   ├── SidebarComponent.axaml              # Dynamic collapsible side tool window
│   ├── EditorComponent.axaml               # Center document text editor workspace
│   ├── FooterComponent.axaml               # Dynamic collapsible bottom output panel
│   ├── StatusBarComponent.axaml            # Persistent bottom desktop status bar
│   └── ToolWindowHeaderComponent.axaml     # Reusable header for tool windows
│
├── Data Table Components:
│   ├── QueryResultTableComponent.axaml     # Professional SQL query results data grid
│   └── SchemaPropertiesTableComponent.axaml # Database table schema & column metadata grid
│
└── Modal & Dialog Components:
    ├── ModalOverlayComponent.axaml         # Universal base modal backdrop & dialog card
    ├── ConnectDatabaseModalComponent.axaml  # SQL Server connection configuration modal
    ├── ConfirmActionModalComponent.axaml   # Action confirmation & danger alert dialog
    └── ExportDataModalComponent.axaml      # Data export format & destination picker modal
```

---

## 3. Data Table Components

### 3.1 `QueryResultTableComponent`
* **File:** [QueryResultTableComponent.axaml](file:///c:/Users/ehsan/Desktop/Fdb/Components/QueryResultTableComponent.axaml) | [QueryResultTableComponent.axaml.cs](file:///c:/Users/ehsan/Desktop/Fdb/Components/QueryResultTableComponent.axaml.cs)
* **DataContext:** `QueryResultTableViewModel`
* **Role:** High-performance, full-featured interactive database query results grid.
* **Features:**
  1. **Alternating Column Colors, Compact Row Height & Explicit Row Numbers:**
     * **Compact Height (25px):** Rows are configured with `RowHeight="25"` (header height 25px, cell padding 8,2) for dense, professional SQL IDE table view without wasted vertical space.
     * Columns alternate between pure white (`#FFFFFF`) and a soft dimmer off-white (`#F8FAFC`) to clearly separate and distinguish vertical data columns.
     * **Row Number Column (`ROW #`):** Column 0 features a dedicated gutter cell (`#F1F5F9`) titled `ROW #` with high-contrast centered bold row numbers (`1`, `2`, `3`, etc.) and a distinct vertical divider line (`2px solid #94A3B8`) in front of every row number, clearly separating the row indices from data.
     * Clean 1px `#CBD5E1` / `#E2E8F0` cell border gridlines.
     * Soft blue highlight (`#EFF6FF`, `#1D4ED8`) on row selection.
  2. **Prominent Column Titles:**
     * Bold column headers (`ROW #`, `NAME`, `ID`, `EMAIL`, `ROLE`, `STATUS`, `CREATED AT`) with `#0F172A` high-contrast typography and subtle 1px border separators.
     * Under the `NAME` header, row names ("Alice Johnson", "Bob Smith", etc.) are displayed clearly.
     * The `STATUS` column displays clean, unboxed text directly.
  3. **Table Header Toolbar:**
     * Search filter box for instant row filtering.
     * Execution duration badge with vector clock icon.
     * Row count badge (e.g. `5 rows`).
     * Action buttons with Microsoft Fluent vector icons: `Refresh` and `Export...`.
  4. **Right-Click ContextMenu:**
     * `Copy Cell Value` (`Ctrl+C`)
     * `Copy Entire Row`
     * `Copy as JSON`
     * `Copy as CSV`
     * `Filter by This Value`
  5. **Table Footer / Pagination:**
     * Status indicator (`Ready • Double-click cell to inspect details`).
     * Pagination navigation controls (`«`, `‹`, `Page 1 of 1`, `›`, `»`).

---

### 3.2 `SchemaPropertiesTableComponent`
* **File:** [SchemaPropertiesTableComponent.axaml](file:///c:/Users/ehsan/Desktop/Fdb/Components/SchemaPropertiesTableComponent.axaml) | [SchemaPropertiesTableComponent.axaml.cs](file:///c:/Users/ehsan/Desktop/Fdb/Components/SchemaPropertiesTableComponent.axaml.cs)
* **DataContext:** `SchemaPropertiesTableViewModel`
* **Role:** Interactive schema and table definition viewer with alternating column striping and row dividers.
* **Features:**
  1. **Compact Row Height (25px) & Explicit Row Numbers (`ROW #`):**
     * Col 0 (`ROW #`) displays the ordinal row number (`1`, `2`, `3`, etc.) with high-contrast centered bold font and a 2px vertical dividing line in front of it.
     * Configured with `RowHeight="25"` for a compact, clean look.
     * Columns alternate between `#FFFFFF` and `#F8FAFC`.
  2. **Schema Header Toolbar:**
     * Filter columns search input.
     * `Copy Column Names` button with vector copy icon.
     * `Generate DDL` button with vector code icon.
  3. **Columns Metadata Grid:**
     * Key indicators: Primary Key (vector checkmark) and Foreign Key (vector link) badges.
     * Titles: `ROW #`, `KEY`, `COLUMN NAME`, `DATA TYPE`, `LENGTH`, `NULLABLE`, `DEFAULT`, `DESCRIPTION`.

---

## 4. Modal & Dialog Components

All modals in Fdb Studio feature:
* **Interactive Dismissal:** Clicking the dark semi-transparent backdrop dismisses the dialog immediately.
* **Keyboard Navigation:** Pressing `Escape` anywhere in the application dismisses any open modal dialog cleanly.
* **Z-Index Elevation:** Configured with top-level `ZIndex="100+"` in `MainWindow.axaml` ensuring modals always appear elevated above editor canvases, splitters, and data grids.

### 4.1 `ModalOverlayComponent` (Base Modal Container)
* **File:** [ModalOverlayComponent.axaml](file:///c:/Users/ehsan/Desktop/Fdb/Components/ModalOverlayComponent.axaml) | [ModalOverlayComponent.axaml.cs](file:///c:/Users/ehsan/Desktop/Fdb/Components/ModalOverlayComponent.axaml.cs)
* **Role:** Universal reusable modal overlay with semi-transparent backdrop and elevated card. Supports hosting arbitrary XAML children via `[Content] public object? ModalContent`.
* **StyledProperties:**
  * `IsOpen` (`bool`): Controls modal visibility and backdrop.
  * `Title` (`string`): Modal card header title.
  * `Subtitle` (`string?`): Optional explanatory text below title.
  * `Icon` (`string`): Emoji or glyph icon in the header badge.
  * `CloseCommand` (`ICommand?`): Invoked on backdrop click, `✕` close button, or cancel.
  * `DialogWidth` (`double`, default: `520`): Width of the dialog card.
  * `PrimaryButtonText` (`string?`): Label for primary action button (e.g. `"Save"`, `"OK"`).
  * `PrimaryCommand` (`ICommand?`): Bound to primary action.
  * `SecondaryButtonText` / `SecondaryCommand`: Optional left action button.
  * `CancelButtonText` (`string`, default: `"Cancel"`).
  * `ModalContent` (`object?`): Content slot for custom body content.

---

### 4.2 `ConnectDatabaseModalComponent`
* **File:** [ConnectDatabaseModalComponent.axaml](file:///c:/Users/ehsan/Desktop/Fdb/Components/ConnectDatabaseModalComponent.axaml) | [ConnectDatabaseModalComponent.axaml.cs](file:///c:/Users/ehsan/Desktop/Fdb/Components/ConnectDatabaseModalComponent.axaml.cs)
* **DataContext:** `ConnectDatabaseModalViewModel`
* **Role:** Dedicated SQL Server connection dialog.
* **Triggers:**
  * Top Navbar `File` $\rightarrow$ `Connect to Database...` (`Ctrl+Shift+C`).
  * Top Navbar toolbar `Connect...` button.
  * Object Explorer (Sidebar) empty state `Connect to Database...` button.
* **Fields:**
  * Server Host / IP (default: `localhost`) & Port (default: `1433`).
  * Initial Database (default: `master`).
  * Authentication method dropdown (`SQL Server Authentication`, `Windows Authentication`, `Microsoft Entra`).
  * Username & masked Password.
  * `Trust Server Certificate` encryption checkbox.
  * Live status alert badge (displays test connection success or error details).
  * Action buttons: `Test Connection`, `Cancel`, `Connect`.

---

### 4.3 `ConfirmActionModalComponent`
* **File:** [ConfirmActionModalComponent.axaml](file:///c:/Users/ehsan/Desktop/Fdb/Components/ConfirmActionModalComponent.axaml) | [ConfirmActionModalComponent.axaml.cs](file:///c:/Users/ehsan/Desktop/Fdb/Components/ConfirmActionModalComponent.axaml.cs)
* **DataContext:** `ConfirmActionModalViewModel`
* **Role:** Danger and confirmation dialog for destructive or session-terminating actions (e.g. `Disconnect`, `DROP TABLE`, `TRUNCATE`).
* **Triggers:**
  * Top Navbar toolbar `Disconnect` button.
* **Features:**
  * Amber/Red danger icon badge with Fluent vector alert symbol.
  * Detailed warning explanation.
  * Highlighted target item badge (e.g. `localhost:1433 (master)`).
  * Red danger confirm button (`#DC2626`).

---

### 4.4 `ExportDataModalComponent`
* **File:** [ExportDataModalComponent.axaml](file:///c:/Users/ehsan/Desktop/Fdb/Components/ExportDataModalComponent.axaml) | [ExportDataModalComponent.axaml.cs](file:///c:/Users/ehsan/Desktop/Fdb/Components/ExportDataModalComponent.axaml.cs)
* **DataContext:** `ExportDataModalViewModel`
* **Role:** Data export configuration dialog.
* **Triggers:**
  * Query Results Table toolbar `Export...` button.
  * Query Results Table right-click context menu `Export Results to File...`.
  * Top Navbar `File` $\rightarrow$ `Export Results to File...`.
* **Features:**
  * Format selector dropdown: `CSV`, `JSON`, `SQL (INSERT INTO)`, `TSV`.
  * Destination file path selector with native OS `Browse...` file picker integration (`SaveFilePickerAsync`).
  * `Include column headers in export` option checkbox.
  * `Cancel` and `Export Data` action buttons.

---

## 5. Layout & Workspace Components

### 5.1 `NavbarComponent`
* **File:** [NavbarComponent.axaml](file:///c:/Users/ehsan/Desktop/Fdb/Components/NavbarComponent.axaml) | [NavbarComponent.axaml.cs](file:///c:/Users/ehsan/Desktop/Fdb/Components/NavbarComponent.axaml.cs)
* **Role:** Fixed top navigation bar.
* **Features:**
  * Native MenuBar (`File`, `Edit`, `View`, `Window`, `Help`).
  * `File` $\rightarrow$ `Connect to Database...` (`Ctrl+Shift+C`) opens `ConnectDatabaseModal`.
  * `Window` menu provides one-click restoration of any closed tab or panel.
  * Run / Query toolbar with quick `⚡ Connect...` trigger.

### 5.2 `SidebarComponent`
* **File:** [SidebarComponent.axaml](file:///c:/Users/ehsan/Desktop/Fdb/Components/SidebarComponent.axaml) | [SidebarComponent.axaml.cs](file:///c:/Users/ehsan/Desktop/Fdb/Components/SidebarComponent.axaml.cs)
* **Role:** Left resizable panel (bounded: `160px – 450px`).
* **Features:** Auto-expanding tabs via `<UniformGrid Rows="1" />`, right-click context menu, and tree container.

### 5.3 `EditorComponent`
* **File:** [EditorComponent.axaml](file:///c:/Users/ehsan/Desktop/Fdb/Components/EditorComponent.axaml) | [EditorComponent.axaml.cs](file:///c:/Users/ehsan/Desktop/Fdb/Components/EditorComponent.axaml.cs)
* **DataContext:** `EditorViewModel`
* **Role:** Central SQL query editor (`1*` star sizing).
* **Features:**
  1. **Independent Tab Content & Persistent Undo History:**
     * Each tab maintains its own independent `TextDocument` with an isolated, persistent `UndoStack`.
     * Switching tabs seamlessly preserves all edits and undo history per tab.
  2. **Undo & Redo Handlers (Ctrl+Z & Ctrl+Y / Ctrl+Shift+Z):**
     * Built-in explicit key handling ensures `Ctrl+Z` smoothly reverts edits back to earlier versions, and `Ctrl+Y` / `Ctrl+Shift+Z` redoes changes.
  3. **Active Tab Blue Indicator:**
     * Active tab displays a prominent 2.5px royal blue accent line (`#2563EB`) at the top.
     * Tab title turns bold royal blue (`#2563EB`).
     * Tab container transitions to pure white (`#FFFFFF`) canvas.
     * Inactive tabs stay muted (`#64748B`, `#F8FAFC`).
  4. **Auto-Expanding Tab Strip:** Built with `<UniformGrid Rows="1" />` ensuring tabs expand to 100% width and contract smoothly when tabs are closed.
  5. **SQL Syntax Highlighting in Blue:** Powered by `SqlKeywordColorizer` reading keywords directly from `Assets/sql_keywords.json`.
  6. **Focus-Safe Auto-Completion & IntelliSense:**
     * Non-disruptive autocomplete: safely triggers on manual request (`Ctrl+Space`) or automatically after typing 3+ keyword letters without stealing editor focus or dropping the caret.
     * `Escape` dismisses completion window and safely restores focus to `CodeEditor.TextArea`.
     * Shows SQL keywords and code snippets with descriptions and one-click insertion.

### 5.4 `FooterComponent`
* **File:** [FooterComponent.axaml](file:///c:/Users/ehsan/Desktop/Fdb/Components/FooterComponent.axaml) | [FooterComponent.axaml.cs](file:///c:/Users/ehsan/Desktop/Fdb/Components/FooterComponent.axaml.cs)
* **Role:** Bottom resizable results panel (bounded: `80px – 450px`).
* **Features:** Auto-expanding tabs (`Terminal`, `Output`, `Problems`), right-click context menu, and embedded `QueryResultTableComponent`.

### 5.5 `StatusBarComponent`
* **File:** [StatusBarComponent.axaml](file:///c:/Users/ehsan/Desktop/Fdb/Components/StatusBarComponent.axaml) | [StatusBarComponent.axaml.cs](file:///c:/Users/ehsan/Desktop/Fdb/Components/StatusBarComponent.axaml.cs)
* **Role:** Persistent status bar anchored at the bottom of the window (`Row 2`).

### 5.6 `ToolWindowHeaderComponent`
* **File:** [ToolWindowHeaderComponent.axaml](file:///c:/Users/ehsan/Desktop/Fdb/Components/ToolWindowHeaderComponent.axaml) | [ToolWindowHeaderComponent.axaml.cs](file:///c:/Users/ehsan/Desktop/Fdb/Components/ToolWindowHeaderComponent.axaml.cs)
* **Role:** Reusable header bar with `Title` and `CloseCommand`.

---

## 6. SQL Keywords, IntelliSense & Syntax Highlighting

Fdb Studio features a data-driven SQL completion and syntax highlighting engine:

* **Keywords Configuration:** [Assets/sql_keywords.json](file:///c:/Users/ehsan/Desktop/Fdb/Assets/sql_keywords.json)
  * Over 110 SQL keywords (DDL, DML, Clauses, Joins, Aggregates, Types, System Functions).
  * Common SQL snippets (`SELECT * FROM`, `CREATE TABLE`, `INSERT INTO VALUES`, `UPDATE SET WHERE`, etc.).
* **Service:** [Services/SqlKeywordsService.cs](file:///c:/Users/ehsan/Desktop/Fdb/Services/SqlKeywordsService.cs)
  * Fast JSON parser with fallback keywords and compiled regex builder.
* **Colorizer:** [Services/SqlKeywordColorizer.cs](file:///c:/Users/ehsan/Desktop/Fdb/Services/SqlKeywordColorizer.cs)
  * High-performance `DocumentColorizingTransformer` for `AvaloniaEdit` coloring all keywords from the JSON in royal blue (`#2563EB`).
* **IntelliSense Autocomplete:** [Services/SqlCompletionData.cs](file:///c:/Users/ehsan/Desktop/Fdb/Services/SqlCompletionData.cs)
  * Triggered automatically upon typing or via `Ctrl+Space`.
  * Integrates with `AvaloniaEdit.CodeCompletion.CompletionWindow`.

---

## 8. Splash Screen & Application Startup

Fdb Studio features a modern desktop startup splash screen:

* **View:** [Views/SplashScreenWindow.axaml](file:///c:/Users/ehsan/Desktop/Fdb/Views/SplashScreenWindow.axaml) | [Views/SplashScreenWindow.axaml.cs](file:///c:/Users/ehsan/Desktop/Fdb/Views/SplashScreenWindow.axaml.cs)
* **ViewModel:** [ViewModels/SplashScreenViewModel.cs](file:///c:/Users/ehsan/Desktop/Fdb/ViewModels/SplashScreenViewModel.cs)
* **Features:**
  1. **Frameless Floating Card:** Borderless (`WindowDecorations="None"`), centered card with deep elevation shadow and gradient accent banner.
  2. **Branding & Highlights:**
     * Glowing vector flame icon badge.
     * App title (`Fdb Studio`) and version badge (`v1.0.0 Developer Preview`).
     * Tagline and feature pill badges.
  3. **3-Second Simulated Progression:**
     * Smooth 4-stage sequence transitioning over 3000ms:
       1. `25%` - Initializing Core Runtime & Workspace...
       2. `50%` - Loading SQL Keywords, IntelliSense & Syntax Highlighting...
       3. `75%` - Scanning Database Drivers & Local Instances...
       4. `100%` - Starting Studio Environment...
  4. **Smooth Dismissal:**
     * Automatically transitions to `MainWindow`.
     * Users can click anywhere on the splash screen or press `Space`/`Enter`/`Escape` to dismiss immediately without waiting.
  5. **CLI Bypass:**
     * Launching with `--no-splash` immediately opens `MainWindow` without displaying the splash screen.

* **Closing Footer:** Text editor expands downwards to occupy 100% vertical space.
* **Closing Sidebar:** Both text editor and footer expand leftwards to occupy 100% horizontal space.
* **Closing Tabs:** Remaining tabs inside `<UniformGrid Rows="1" />` automatically grow to fill the tab bar.
* **Restoring via Window Menu:** Restores previous bounded dimensions (`160-450px` sidebar, `80-450px` footer).
* **Splitter Dragging:** `DragCompleted` ensures center areas retain `1*` (Star) sizing.

---

## 8. MVVM ViewModels Structure

```
MainViewModel
├── Navbar: NavbarViewModel
├── Sidebar: SidebarViewModel
├── Editor: EditorViewModel
│     ├── SelectedTab: EditorTabItem
│     └── Tabs: ObservableCollection<EditorTabItem>
├── Footer: FooterViewModel
│
├── Modals:
│   ├── ConnectModal: ConnectDatabaseModalViewModel
│   ├── ConfirmModal: ConfirmActionModalViewModel
│   └── ExportModal: ExportDataModalViewModel
│
└── Tables:
    ├── SampleQueryResult: QueryResultTableViewModel
    └── SampleSchema: SchemaPropertiesTableViewModel
```
