# File Sorter Documentation

Welcome to the documentation for File Sorter. This guide explains how to set up, configure, and run the application to automatically organize your files.

[Back to Main README](../README.md)

## Table of Contents

- [Prerequisites](#prerequisites)
- [Setup](#setup)
- [Configuration (`config.yaml`)](#configuration-configyaml)
  - [Top-Level Settings](#top-level-settings)
  - [Defining Rules](#defining-rules)
  - [Rule Types and Parameters](#rule-types-and-parameters)
    - [`creation_date`](#creation_date)
    - [`last_modified_date`](#last_modified_date)
    - [`file_size`](#file_size)
    - [`regex`](#regex)
  - [Target Path Placeholders](#target-path-placeholders)
  - [Example Configuration](#example-configuration)
- [Running the Application](#running-the-application)
- [Understanding the Output](#understanding-the-output)
- [Simulation Mode](#simulation-mode)
- [Troubleshooting](#troubleshooting)

## Prerequisites

Before you begin, ensure you have the following installed:

1.  **.NET Runtime:** The application is built using .NET. You need the .NET Runtime (version 6.0 or later recommended) installed on your system. You can download it from the [official .NET website](https://dotnet.microsoft.com/download).
2.  **Application Files:** You need the compiled File Sorter executable (e.g., `FileSorter.dll` or `FileSorter.exe` depending on your build) and the `config.yaml` file.

## Setup

1.  **Obtain the Application:**
    *   **Build from Source:** If you have the source code, navigate to the project directory in your terminal and run `dotnet build --configuration Release`. The output will typically be in a subfolder like `bin/Release/netX.Y/`.
    *   **Download Release:** (If applicable) Download the latest pre-compiled release from the project's releases page.
2.  **Place Configuration:** Create a `config.yaml` file (or use the example provided) and place it in the **same directory** as the File Sorter executable. Alternatively, you can specify a different path to the config file when running the application.

## Configuration (`config.yaml`)

This is the core of the application. You define where to look for files and how to sort them in the `config.yaml` file. The file uses YAML syntax.

### Top-Level Settings

-   `source_directories`: (Required) A list of paths to the folders where the application should look for files to sort.
    ```yaml
    source_directories:
      - /Users/yourname/Downloads
      - C:\ToSort
      - ./RelativeFolderToSort # Path relative to where the app runs
    ```
-   `rules`: (Required) A list of rule objects that define the sorting logic. See [Defining Rules](#defining-rules) below.
-   `conflict_resolution`: (Optional, Default: `error`) Defines how to handle cases where a single file matches multiple rules. Options:
    *   `first_match`: The file is processed by the *first* rule in the `rules` list that it matches.
    *   `error`: The application logs the conflict and stops processing *before* moving any files if potential rule conflicts are detected during startup (based on simple checks like identical target templates). If a conflict occurs during runtime (a file matches multiple rules), it logs an error and skips the file.
    *   `skip`: If a file matches multiple rules, it is logged and skipped (not moved).
    *   `log`: If a file matches multiple rules, it is logged, and the `first_match` strategy is applied (the first matching rule processes the file).
-   `simulation_mode`: (Optional, Default: `false`) If set to `true`, the application will perform all checks and log what it *would* do, but it will **not** actually move any files or create directories. Useful for testing your configuration.
    ```yaml
    simulation_mode: true
    ```

### Defining Rules

Each item under the `rules` list is an object with the following keys:

-   `name`: (Required) A unique, descriptive name for the rule (used in logging).
-   `type`: (Required) The type of rule to apply. See [Rule Types](#rule-types-and-parameters) below.
-   `params`: (Optional) A dictionary of parameters specific to the rule `type`.
-   `target`: (Required) The destination directory path template where matching files should be moved. Can contain placeholders. See [Target Path Placeholders](#target-path-placeholders).

```yaml
rules:
  - name: "Rule Description"
    type: rule_type_here
    params:
      param1: value1
      param2: value2
    target: /path/to/destination/{placeholder}/
