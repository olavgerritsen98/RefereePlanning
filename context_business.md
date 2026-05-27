# Business Context: Smart Referee Orchestrator (SRO)

## 1. Project Overzicht
SRO is een SaaS-oplossing ontworpen om het toewijzen van scheidsrechters bij amateursportverenigingen te automatiseren. Het combineert een deterministische plannings-engine (Constraint Solver) met een AI-laag (LLM) voor het afhandelen van ongestructureerde communicatie (zoals ziekmeldingen via WhatsApp). 

## 2. MVP Scope (Korfbal - KV Wageningen)
De initiële applicatie wordt gebouwd voor de Nederlandse korfbalmarkt, waarbij data wordt geïmporteerd vanuit de 'Sportlink Club.Dataservice'. 
* **Belangrijkste wijziging in traditionele planning:** De toewijzing van wedstrijden gebeurt in deze MVP **niet op individueel niveau, maar op teamniveau**.
* Het systeem koppelt een te fluiten wedstrijd aan een "Supplier Team". Het Supplier Team is vervolgens zelf verantwoordelijk voor het leveren van een specifiek persoon.

## 3. Entiteiten & Databron
* **Wedstrijden (Matches):** Geïmporteerd via Sportlink URL's. Betreft thuiswedstrijden voor Breedtekorfbal (jeugd, 1 senioren, 3 midweek) en lagere wedstrijdsport (als reserves).
* **Leveranciers (Supplier Teams):** Alle seniorenteams (inclusief midweek) en de oudste jeugdteams (bijv. A- en B-jeugd).

## 4. Business Rules & Constraints (De Plannings-Engine)
Het algoritme moet rekening houden met de volgende harde en zachte regels:

**Harde Eisen (Must-have):**
1.  **Niveau/Leeftijd Matching:** 
    * Jeugdteams (als supplier) worden gekoppeld aan de jongste jeugdteams (als match).
    * Wedstrijdsport-teams (als supplier) fluiten de reservebeurten in het lagere wedstrijdsport. Niveau telt hier mee.
2.  **Tijd-Conflict (Buffer):** Er moet minimaal één volledige wedstrijd (incl. rust/uitloop, ca. 1.5 uur) buffer zitten tussen de tijd dat een team zelf speelt en de tijd dat ze moeten fluiten.
3.  **Reistijd:** Als een supplier-team een uitwedstrijd speelt, moet reistijd worden meegerekend in de buffer.

**Zachte Eisen (Fairness - Nice-to-have):**
1.  **Eerlijke verdeling:** Het algoritme streeft ernaar om alle supplier-teams gedurende het seizoen ongeveer evenveel fluitbeurten te geven.

## 5. De AI-Use Case (Communicatie)
Als een team een wijziging doorgeeft ("Jan van Sen 3 is ziek"), plakt de coördinator dit in de app. De AI moet deze tekst parsen, begrijpen dat het om team "Sen 3" gaat, de gekoppelde wedstrijd opzoeken, en een webhook of actie voorstellen om het intern binnen dat team op te lossen, of (in extremis) het algoritme vragen een nieuw team te zoeken.