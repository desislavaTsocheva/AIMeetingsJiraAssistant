# 🚀 Jira AI: Meeting Assistant Manager

**Jira AI Meeting Assistant** is a privacy-first productivity tool that bridges spoken meeting discussions with actionable project management. Using **local AI (Ollama)** and **speech-to-text (Whisper)**, the app analyzes meeting transcripts to automatically extract action items and sync them directly with **Jira / Atlassian Cloud**.

---

## 🌟 Key Features

* **Local AI Processing**
  Runs **Llama 3 (8B)** via **Ollama** for on-prem, privacy-focused transcript analysis—your meeting data never leaves your infrastructure.

* **Speech-to-Text Integration**
  Converts meeting recordings into accurate transcripts using **Whisper**.

* **Secure Jira Integration**
  Implements **OAuth 2.0 (Authorization Code Flow)** for secure access to Atlassian Cloud resources.

* **Action Item Extraction**
  Intelligent parsing of transcripts to identify **tasks, assignees, and deadlines** using **Semantic Kernel**.

* **Enterprise-Grade Security**
  Sensitive credentials and tokens are protected using the **Microsoft Data Protection API (AES-256)**.

* **Smart Setup**
  Built-in **QR code generation** for quick navigation to Jira security and authorization settings.

---

## 🛠 Tech Stack & Tools

### Core Framework & UI

* **ASP.NET Core Blazor (Interactive Server)** – High-performance, C#-based interactive web UI
* **C# / .NET 8** – Primary language and runtime
* **NavigationManager & Query String Parsing** – Complex state handling and deep-linking across auth flows

### Artificial Intelligence

* **Ollama (Llama 3: 8B)** – Local LLM orchestration
* **Semantic Kernel** – SDK for integrating AI models into C# applications
* **Whisper** – High-accuracy audio transcription

### Data & Persistence

* **SQLite** – Lightweight, serverless relational database
* **Entity Framework Core (EF Core)** – ORM for database access
* **LINQ & Lambda Expressions** – Clean and efficient querying
* **CRUD Operations** – Full lifecycle management of users, sessions, and tasks

### Networking & Security

* **OAuth 2.0** – Secure authorization with Atlassian Cloud
* **Data Encryption** – Symmetric encryption for Jira tokens at rest
* **IHttpClientFactory** – Optimized HTTP client management
* **Secrets Management** – Environment-aware handling of client IDs and secrets

---

## 🏗 Architecture

The application follows a **Service-Oriented Architecture (SOA)** and leverages **Dependency Injection (DI)** to maintain a decoupled, testable, and maintainable codebase.

### Core Services

* **AtlassianAuthService**
  Manages OAuth 2.0 authorization flows and token exchanges.

* **JiraService**
  Handles Jira REST API communication (issue creation, user search, metadata access).

* **UserSession**
  Manages scoped state and authentication context across the Blazor Server circuit.

* **FileProcessingService**
  Processes meeting artifacts, including audio transcripts and PDF/Word documents.

---

## 🔒 Security First

* **Zero-Knowledge AI**
  Local LLM usage ensures sensitive meeting discussions remain private.

* **Encrypted Storage**
  All user credentials stored in SQLite are encrypted using system-level keys.

* **Concurrency Handling**
  Optimized SQLite access patterns under Blazor Server to prevent database locking during high-frequency I/O operations.

---

## 📌 Use Cases

* Automatically turn meeting discussions into Jira tasks
* Reduce manual note-taking and backlog grooming
* Keep sensitive enterprise data fully on-prem

---


