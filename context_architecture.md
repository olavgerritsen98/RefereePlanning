# Software Architecture & Setup: Smart Referee Orchestrator (SRO)

## 1. Technologie Stack
*   **Backend / Core API:** .NET 10 (C#) - ASP.NET Core Web API.
*   **Frontend:** Blazor WebAssembly (WASM) - Hosted as Azure Static Web App.
*   **Background Processing:** Azure Functions (Isolated Worker Model, afgestemd op .NET 10).
*   **Database:** Azure SQL Database.
*   **ORM:** Entity Framework Core 10.
*   **Authentication:** Azure AD B2C (E-mail/Password, JWT Bearer tokens).
*   **AI Integration:** Azure OpenAI Services (GPT-4o / GPT-4o-mini via Azure SDK).

## 2. Architectuur Patronen
De solution volgt **Clean Architecture** principes om domeinlogica te isoleren van infrastructuur.
*   **Multi-Tenancy:** Logical isolation via een `TenantId` (ClubId) op database-niveau, afgedwongen via **EF Core Global Query Filters**.
*   **Polymorfisme voor Toewijzing:** Om schaalbaar te blijven naar individuele toewijzingen (voor andere sporten in de toekomst), gebruikt het domein een `IRefereeSupplier` interface.

## 3. Solution Structuur
De .NET solution (`SRO.sln`) is opgedeeld in de volgende projecten:

1.  **`SRO.Domain` (Class Library):**
    *   Geen dependencies naar buiten.
    *   Bevat Entiteiten (`Match`, `TeamSupplier`, `IndividualReferee`).
    *   Bevat Interfaces (`IRefereeSupplier`, `IAssignmentConstraint`, `ISportlinkRepository`).
    *   Bevat het planningsalgoritme (`AssignmentEngine`).
2.  **`SRO.Infrastructure` (Class Library):**
    *   Afhankelijk van `SRO.Domain`.
    *   Bevat `SroDbContext` (Entity Framework Core 10 setup incl. Tenant filters).
    *   Bevat `SportlinkClient` (HTTP calls naar Sportlink data-services).
    *   Bevat `OpenAIService` (Implementatie van AI-parsing logic).
3.  **`SRO.Api` (ASP.NET Core Web API):**
    *   Afhankelijk van Domain en Infrastructure.
    *   Bevat Controllers, Auth setup (JWT validatie), en Dependency Injection (DI) configuratie.
4.  **`SRO.Functions` (Azure Functions):**
    *   Bevat `NightlySyncFunction` (TimerTrigger voor ophalen Sportlink data).
    *   Deelt de DbContext en business logic met de API.
5.  **`SRO.Shared` (Class Library):**
    *   Bevat DTO's en Enums gedeeld tussen de Web API en Blazor Frontend.
6.  **`SRO.Web` (Blazor WebAssembly):**
    *   Frontend UI, aanroepen van API via `HttpClient`.

## 4. Kern Entiteiten & Datamodel (Concept)
```csharp
// De abstractie voor schaalbaarheid
public interface IRefereeSupplier
{
    string SupplierId { get; } // Sportlink ID
    string DisplayName { get; }
    string TenantId { get; }
}

public class TeamSupplier : IRefereeSupplier
{
    public string SupplierId { get; set; }
    public string DisplayName { get; set; } // e.g., "Senioren 3"
    public string TenantId { get; set; }
    public int MatchesAssignedThisSeason { get; set; }
}

public class Match
{
    public string MatchId { get; set; }
    public string MatchCode { get; set; } // e.g., "F1" of "Senioren 4"
    public DateTime StartTime { get; set; }
    public string TenantId { get; set; }
    
    // Kan null zijn totdat engine draait
    public IRefereeSupplier AssignedSupplier { get; set; } 
    public bool IsReserveAssignment { get; set; } // True voor KNKV wachtkamer
}