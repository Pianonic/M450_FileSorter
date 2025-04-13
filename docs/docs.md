# File Sorter Documentation

This guide explains how to set up, configure, and run the File Sorter application.

[Back to Main README](../README.md)

## Table of Contents

-   [Prerequisites](#prerequisites)
-   [Setup](#setup)
-   [Configuration (`config.yaml`)](#configuration-configyaml)
    -   [Top-Level Settings](#top-level-settings)
    -   [Defining Rules](#defining-rules)
    -   [Rule Types and Parameters](#rule-types-and-parameters)
        -   [`creation_date`](#creation_date)
        -   [`last_modified_date`](#last_modified_date)
        -   [`file_size`](#file_size)
        -   [`regex`](#regex)
    -   [Target Path Placeholders](#target-path-placeholders)
    -   [Example Configuration](#example-configuration)
-   [Running the Application](#running-the-application)
-   [Understanding the Output](#understanding-the-output)
-   [Simulation Mode](#simulation-mode)

## Prerequisites

1.  **.NET Runtime:** Version 6.0 or later. Download from the [official .NET website](https://dotnet.microsoft.com/download).
2.  **Application Files:** The compiled `FileSorter.dll` (or executable) and your `config.yaml`.

## Setup

1.  **Get Application:** Build from source (`dotnet build --configuration Release`) or download a release.
2.  **Place Configuration:** Put your `config.yaml` file in the same directory as `FileSorter.dll`.

## Configuration (`config.yaml`)

This YAML file defines the sorting behavior.

### Top-Level Settings

-   `source_directories`: (Required) List of folders (absolute or relative paths) to scan for files.
    ```yaml
    source_directories:
      - /Users/yourname/Downloads
      - ./ToSort
    ```
-   `rules`: (Required) List of sorting rule objects. See below.
-   `conflict_resolution`: (Optional, Default: `error`) How to handle a file matching multiple rules:
    *   `first_match`: Use the first matching rule found in the list.
    *   `error`: Log conflict and skip the file (may also stop app on startup if conflicts like identical targets are found).
    *   `skip`: Log conflict and skip the file.
    *   `log`: Log conflict, then use `first_match`.
-   `simulation_mode`: (Optional, Default: `false`) If `true`, logs intended actions but **does not** move files or create directories. Use this for testing.
    ```yaml
    simulation_mode: true
    ```

### Defining Rules

Each item under `rules` defines one rule:

-   `name`: (Required) Unique name for logging (e.g., `"Sort JPGs"`).
-   `type`: (Required) Rule type (e.g., `creation_date`, `regex`). See below.
-   `params`: (Optional) Dictionary of parameters specific to the `type`.
-   `target`: (Required) Destination directory path (absolute or relative), can use placeholders.

### Rule Types and Parameters

**Common Parameter (Available for all rule types):**

-   `pattern` (in `params`): (Optional) A Regex string (case-insensitive) to filter filenames *before* applying the main rule logic. If the filename doesn't match, the rule is skipped for that file.
    ```yaml
    params:
      pattern: '\.log$' # Only consider .log files for this rule
    ```

**Specific Rule Types:**

#### `creation_date`

Sorts by file creation date.

-   **Specific `params`:** None implemented beyond the common `pattern`.
-   **Placeholders:** `{yyyy}`, `{MM}`, `{dd}` (from creation date).
-   **Example:**
    ```yaml
    - name: "Photos by Creation Year"
      type: creation_date
      params:
        pattern: '\.(jpg|jpeg|png)$'
      target: ./Photos/{yyyy}/
    ```

#### `last_modified_date`

Sorts by file last modified date.

-   **Specific `params`:**
    -   `newer_than_days`: (Optional) Integer. File must be modified within this many days (inclusive).
-   **Placeholders:** `{yyyy}`, `{MM}`, `{dd}` (from last modified date).
-   **Example:**
    ```yaml
    - name: "Recent Docs"
      type: last_modified_date
      params:
        pattern: '\.docx$'
        newer_than_days: 30 # Modified in the last 30 days
      target: ./RecentDocs/{yyyy}-{MM}/
    ```

#### `file_size`

Sorts by file size.

-   **Specific `params`:**
    -   `min_size_mb`: (Optional) Number. Minimum file size in Megabytes (inclusive).
    *   `max_size_mb`: (Optional) Number. Maximum file size in Megabytes (inclusive).
-   **Placeholders:** None specific.
-   **Example:**
    ```yaml
    - name: "Large Archives"
      type: file_size
      params:
        pattern: '\.zip$'
        min_size_mb: 100 # At least 100 MB
      target: ./LargeArchives/
    ```

#### `regex`

Sorts if filename matches the main Regex pattern. Can use parts of the filename in the target path.

-   **Specific `params`:**
    -   `pattern`: (**Required**) The main Regex string (case-insensitive) for matching. Use `()` for capture groups.
-   **Placeholders:** `{$1}`, `{$2}`, ... (Content of the Nth capture group from the main `pattern`).
-   **Example:**
    ```yaml
    - name: "Invoices YYYY-MM"
      type: regex
      params:
        # $1 = Year, $2 = Month
        pattern: '^Inv_(\d{4})-(\d{2})_.*\.pdf$'
      target: ./Invoices/{$1}/{$2}/
    ```

### Target Path Placeholders

-   `{yyyy}`, `{MM}`, `{dd}`: Year, Month, Day (for `creation_date`, `last_modified_date`).
-   `{$1}`, `{$2}`, ...: Nth capture group from `regex` rule's main `pattern`.

### Example Configuration

```yaml
# config.yaml
source_directories:
  - ./ToSort
conflict_resolution: first_match
# simulation_mode: true

rules:
  - name: "Recent Text Files"
    type: last_modified_date
    params:
      pattern: '\.txt$'
      newer_than_days: 7
    target: ./RecentText/{yyyy}-{MM}-{dd}/

  - name: "Project Reports (ProjName_YYYY)"
    type: regex
    params:
      # $1 = Project Name, $2 = Year
      pattern: '^([A-Za-z]+)_Report_(\d{4})\.pdf$'
    target: ./Projects/{$1}/Reports/{$2}/

  - name: "Large Backup Files (>50MB)"
    type: file_size
    params:
      min_size_mb: 50
    target: ./Backups/Large/

  - name: "All Other JPEGs (by creation year)"
    type: creation_date
    params:
      pattern: '\.jpe?g$'
    target: ./Images/Misc/{yyyy}/
```

## Running the Application

1.  **Open Command Prompt or PowerShell:**
    *   You can search for "cmd" or "PowerShell" in the Windows Start menu.
2.  **Navigate to the Application Directory:**
    *   Use the `cd` command to change to the folder where you placed `M450_FileSorter.exe` and your YAML configuration file.
    *   Example: `cd C:\Users\YourName\Desktop\FileSorterApp`
3.  **Run the Executable:**
    *   **To use `config.yaml` (if it's in the same directory):**
        ```cmd
        M450_FileSorter.exe
        ```
    *   **To specify a different YAML file (e.g., `my_rules.yaml`) located in the same directory:**
        ```cmd
        M450_FileSorter.exe my_rules.yaml
        ```
    *   **To specify a YAML file in a different directory:**
        ```cmd
        M450_FileSorter.exe C:\Path\To\Your\custom_config.yaml
        ```

## Understanding the Output

The console shows:

-   Config file used, simulation status, conflict strategy.
-   Loaded rules or config errors.
-   Startup conflict detection results.
-   Which source directory is being processed.
-   Multi-match conflict info and resolution action (`Applying first...`, `Skipping...`).
-   `[Simulate]` logs if simulation mode is on.
-   `Moving '...' to '...'` or `Created directory: ...` messages.
-   Error messages (permissions, invalid config, etc.).
-   Completion message.

## Simulation Mode

Enable with `simulation_mode: true` in `config.yaml`.

-   **Purpose:** Test your configuration without moving files.
-   **Action:** The app logs everything it *would* do (moves, directory creations) prefixed with `[Simulate]`.
-   **Recommendation:** Always use simulation mode first when changing rules.
