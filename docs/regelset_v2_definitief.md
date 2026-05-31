# Regelset Scheidsrechtertoewijzing — KV Wageningen (v2)

## Status: GEVALIDEERD (antwoorden coördinator Lammert verwerkt, mei 2026)

---

## 1. Scope: Alleen thuiswedstrijden

We plannen **uitsluitend scheidsrechters voor thuiswedstrijden**. Uitwedstrijden zijn de verantwoordelijkheid van de thuisspelende club.

Uitwedstrijden van onze teams zijn wél relevant als **beschikbaarheidsdata**: als een supplier-team uit speelt, is dat team niet beschikbaar om te fluiten (zie §4 bufferberekening).

---

## 2. Supplier Teams (wie levert scheidsrechters?)

### Definitieve lijst

| Categorie | Teams | Opmerkingen |
|-----------|-------|-------------|
| Senioren wedstrijdkorfbal | Wageningen 1, 2, 3, 4, 5 | Allemaal supplier |
| Senioren breedtekorfbal (BK) | Wageningen BK6, BK7, BK8 | **Variabel per periode** (zie §3) |
| Midweek | MW1, MW2, MW3 | Spelen di/wo avond |
| Jeugd wedstrijdkorfbal | U19-1, U19-2 | Oudste jeugd |
| Jeugd breedtekorfbal | J1 | Oudste breedte-jeugd |

### Wie levert GEEN scheidsrechter

| Categorie | Reden |
|-----------|-------|
| A-jeugd (A1–A5) | Te jong / niet ingeschakeld |
| U17, U15 | Te jong / niet ingeschakeld |
| J2, J3, J4+ | Te jong |
| Alle B/C/D/E/F teams | Te jong |

### Variabele samenstelling BK-teams per periode

Het aantal BK-teams wisselt per competitieperiode:
- Najaar-veld: bijv. BK6, BK7, BK8
- Zaalcompetitie: vaak +1 extra team (bijv. BK6, BK7, BK8, BK9)
- Voorjaar-veld: soms minder (bijv. alleen BK6)

De coördinator configureert per verdelingsronde welke teams actief zijn.

---

## 3. Verdelingsrondes (4 per seizoen)

Het seizoen is verdeeld in **vier periodes**. Per periode wordt een verdelingsronde gedraaid:

| # | Periode | Typisch tijdvak |
|---|---------|-----------------|
| 1 | Najaar-veld | september – november |
| 2 | Zaal 1e helft | december – januari |
| 3 | Zaal 2e helft | februari – maart |
| 4 | Voorjaar-veld | april – juni |

Per ronde:
- Bepaalt de coördinator welke supplier-teams actief zijn
- Worden alle thuiswedstrijden zonder scheidsrechter in die periode verdeeld
- Is de verdeling **gelijk**: elk actief team krijgt ≈ evenveel wedstrijden
- Is er **geen maximum** per speelweekend (als het eerlijk uitkomt)

---

## 4. Toewijzingsregels

### 4.1 Harde Constraints (automatisch uitsluiten)

#### Regel H1: Tijdconflict — eigen wedstrijd

Een team mag **niet** worden ingepland als ze zelf spelen en er een tijdconflict is.

**Bufferberekening bij thuiswedstrijd van supplier:**
- Team niet beschikbaar van `aanvang_eigen_wedstrijd - 30 min` tot `aanvang_eigen_wedstrijd + 1,5 uur`

**Bufferberekening bij uitwedstrijd van supplier:**
- Team moet 45 minuten voor aanvang aanwezig zijn bij uitlocatie
- Vertrek = `aanvang_uitwedstrijd - reistijd - 45 min`
- Terugkomst = `aanvang_uitwedstrijd + 1,5 uur + reistijd`
- Team niet beschikbaar van `vertrek` tot `terugkomst`

#### Regel H2: Niet dubbel inplannen

Een team kan maximaal 1 fluitbeurt per tijdslot hebben (als er overlap zou zijn met een andere fluitbeurt).

#### Regel H3: KNKV-wedstrijden uitsluiten

Wedstrijden van **Wageningen 1, 2, 3** en **U19-1** worden altijd voorzien van een KNKV-scheidsrechter. Hiervoor plannen we **geen** eigen scheidsrechter (ook niet als reserve).

Overige wedstrijden waar het `scheidsrechters`-veld al gevuld is in Sportlink → ook overslaan.

---

### 4.2 Zachte Constraints (voorkeuren / scoring)

#### Regel Z1: Eerlijke verdeling (hoogste prioriteit)

Binnen een ronde krijgt elk actief supplier-team ≈ evenveel wedstrijden.
- **Implementatie:** Team met de minste toegewezen beurten in deze ronde krijgt voorrang.
- Maximaal verschil: 1 wedstrijd tussen teams.

#### Regel Z2: Niveau-voorkeur (matching)

In principe kan **elk** supplier-team **elke** wedstrijd fluiten. Maar er is een voorkeursvolgorde:

| Wedstrijd-type | Voorkeur supplier | Reden |
|----------------|-------------------|-------|
| Reserve Ereklasse / Overgangsklasse / 1e klasse / 2e klasse | Wageningen 1–5 | Hoger niveau ervaring nodig |
| B4-wedstrijden (jongste jeugd) | BK-teams, U19-1/2, J1 | Geschikter voor jeugd/BK |
| B-rood/oranje/geel (oudere jeugd) | BK-teams, U19-1/2, J1 | Geschikter voor jeugd/BK |
| Midweek-wedstrijden | Senioren (1–5), MW-teams | MW is minder geschikt voor jeugd/BK |
| Senioren BK wedstrijden | Alle teams gelijk | Geen specifieke voorkeur |

> **Let op:** Dit zijn **voorkeuren** (bonuspunten in scoring), geen harde uitsluiting. Als de voorkeursteams niet beschikbaar zijn, mag elk ander team worden ingepland.

#### Regel Z3: Midweek-geschiktheid

Midweek-wedstrijden (di/wo avond) worden bij voorkeur **niet** toegewezen aan jeugdteams (U19, J1) en BK-teams. Reden: MW-teams bestaan uit spelers die aan het afbouwen zijn; de sfeer en het niveau past minder bij jonge scheidsrechters.

---

## 5. Reistijdberekening

### Beschikbare data uit Sportlink
- `accommodatie` (naam sportpark)
- `plaats` (plaatsnaam)

### Berekening
- Per unieke accommodatie: geocoden (lat/lng opslaan)
- Reistijd berekenen via routeservice (OpenRouteService of Google Maps Distance Matrix)
- Resultaat cachen per route (accommodatie A → accommodatie B = X minuten)
- Eenmalig per seizoen ~50-100 unieke routes; daarna cache hits

### Gebruik in engine
- Als supplier-team een uitwedstrijd heeft:
  - `niet_beschikbaar_vanaf = aanvang_uitwedstrijd - reistijd - 45 min`
  - `niet_beschikbaar_tot = aanvang_uitwedstrijd + 90 min + reistijd`
- Als de te fluiten wedstrijd binnen dit venster valt → team uitgesloten

### Fallback
- Als geocoding/routeberekening faalt: standaard 45 minuten reistijd aannemen

---

## 6. Reserve-mechanisme (wedstrijdkorfbal)

### Context
Voor wedstrijdkorfbal-wedstrijden (niet Wag 1-3, U19-1) weten we pas op **donderdag** voor het speelweekend of de KNKV-scheidsrechter daadwerkelijk is toegewezen.

### Werkwijze
1. Bij het genereren van de verdeling: wedstrijdkorfbal-wedstrijden (die niet auto-excluded zijn) krijgen assignment-type **Reserve**
2. Nightly sync controleert of het `scheidsrechters`-veld in Sportlink is ingevuld
3. Als KNKV een scheidsrechter heeft toegewezen → reserve automatisch gedeactiveerd
4. Donderdag-overzicht: welke reserves moeten **wél** fluiten (KNKV heeft niks ingevuld)

### Verschil met definitieve assignments
| | Definitief | Reserve |
|---|---|---|
| Wanneer | Direct bevestigd | Donderdag bevestigd |
| Typisch voor | BK-wedstrijden, jeugdwedstrijden | Wedstrijdkorfbal (4e/5e team etc.) |
| In Sportlink? | Ja (naam invullen) | Nee (geen toegang) |
| Communicatie | Lijstje naar team | Reservelijst + donderdag-notificatie |

---

## 7. Proces (bevestigd door coördinator)

### Seizoensflow (4x per jaar)

```
┌─────────────────────────────────────────────────────────┐
│ 1. Coördinator maakt verdelingsronde aan                │
│    (selecteert datumbereik + actieve supplier-teams)     │
├─────────────────────────────────────────────────────────┤
│ 2. App genereert verdeling (one-click)                  │
│    - Eerlijk verdeeld                                    │
│    - Conflictvrij (tijdcheck + reistijd)                │
│    - Voorkeuren meegewogen                              │
├─────────────────────────────────────────────────────────┤
│ 3. Coördinator reviewt, eventueel handmatig swappen     │
├─────────────────────────────────────────────────────────┤
│ 4. Export: lijstje per team (PDF/Excel)                 │
│    → Verstuurd naar teambegeleiders                     │
├─────────────────────────────────────────────────────────┤
│ 5. Teams vullen namen in (binnen 1 week)               │
│    → Teams zijn verantwoordelijk                         │
│    → Mogen vervanger buiten team regelen                │
├─────────────────────────────────────────────────────────┤
│ 6. Namen worden ingevoerd in Sportlink (BK-wedstrijden) │
│    of op reservelijst gezet (wedstrijdkorfbal)          │
├─────────────────────────────────────────────────────────┤
│ 7. Wekelijks (donderdag): reserve-check                 │
│    - Sync detecteert KNKV-invulling                     │
│    - Reserves genotificeerd: wel/niet fluiten           │
└─────────────────────────────────────────────────────────┘
```

### Bij uitval/ziekmelding
- Aangewezen speler (of diens team) is verantwoordelijk voor vervanger
- Vervanger wordt doorgegeven aan coördinator → update in Sportlink
- App kan beschikbaarheid tonen: welke teamleden hebben geen conflict?

---

## 8. Sportlink Data — Wat we gebruiken

### Wél beschikbaar en gebruikt

| Data | Bron | Gebruik |
|------|------|---------|
| Thuiswedstrijden + tijden | `programma?thuis=ja` | Te plannen wedstrijden |
| Uitwedstrijden + tijden | `programma?uit=ja` | Beschikbaarheidscheck suppliers |
| Scheidsrechter ingevuld? | `programma` → `scheidsrechters` | Overslaan als al toegewezen |
| Team + klasse + categorie | `teams` | Supplier-identificatie |
| Accommodatie + plaats | `programma` | Reistijdberekening |
| Afgelastingen | `afgelastingen` | Wedstrijden uitsluiten |
| Teamindeling (leden) | `team-indeling` | Wie zit in welk team |

### Niet beschikbaar (oplossing nodig)

| Data | Oplossing |
|------|-----------|
| Coördinaten accommodaties | Geocoding API (eenmalig per locatie) |
| Reistijd tussen locaties | Route API + cache |
| Welke teams supplier zijn | Configuratie per ronde in de app |
| Competitieperiode-grenzen | Coördinator stelt in per ronde |

---

## 9. Engine-algoritme (samenvatting)

```
Input:
  - Thuiswedstrijden zonder scheidsrechter in deze ronde
  - Actieve supplier-teams voor deze ronde
  - Alle wedstrijden van suppliers (thuis + uit) → beschikbaarheidsdata
  - Reistijd-cache

Stap 1: Sorteer te plannen wedstrijden op datum/tijd

Stap 2: Per wedstrijd:
  a) Bepaal beschikbare teams (harde constraints H1, H2, H3)
  b) Score beschikbare teams:
     - Eerlijke verdeling (Z1): team met minste beurten → +10 punten
     - Niveau-voorkeur (Z2): voorkeursteam voor dit type → +3 punten
     - Midweek-geschiktheid (Z3): als midweek-wedstrijd, senioren/MW → +2 punten
  c) Wijs team met hoogste score toe
  d) Update beurtenteller

Stap 3: Markeer wedstrijdkorfbal-assignments als "Reserve"

Output:
  - Lijst assignments (team × wedstrijd × type)
  - Statistieken: verdeling per team, eventuele conflicten
```

### Complexiteit
- ~40-60 wedstrijden per ronde
- ~10-15 actieve supplier-teams
- Greedy algoritme is voldoende (geen OR-solver nodig)
- Draaitijd: < 1 seconde

---

## 10. Voorbeeldscenario

**Situatie:** Zaterdag 12 oktober, thuiswedstrijden zonder scheidsrechter:
- 10:00 — Wageningen J25 vs Opponent (B4-blauw)
- 11:30 — Wageningen J12 vs Opponent (B-rood)
- 14:30 — Wageningen 4 vs Opponent (Reserve 1e klasse) ← wedstrijdkorfbal

**Supplier-teams beschikbaar?**

| Team | Eigen wedstrijd | Beschikbaar voor 10:00? | 11:30? | 14:30? |
|------|----------------|-------------------------|--------|--------|
| Wag 3 | 14:30 thuis | ✅ | ✅ | ❌ (speelt zelf) |
| Wag 5 | 12:00 uit (Arnhem, 25 min) | ✅ | ❌ (vertrek 10:50) | ❌ (terug 14:55) |
| BK6 | Vrij | ✅ | ✅ | ✅ |
| U19-1 | 12:00 thuis | ✅ | ❌ (30 min buffer) | ✅ |
| MW1 | Vrij (speelt doordeweeks) | ✅ | ✅ | ✅ |

**Engine-toewijzing:**
- 10:00 B4-blauw → **BK6** (voorkeur jeugd/BK voor B4, minste beurten)
- 11:30 B-rood → **MW1** (BK6 al ingepland, MW1 beschikbaar, minste beurten)
- 14:30 Reserve 1e kl → **Wag 3** ❌ speelt zelf → **MW1** al beurt → **BK6** voorkeur is jeugd... → **Wag 5** ❌ niet terug → Beschikbaar: BK6, MW1. Voorkeur Reserve-klasse = senioren. MW1 heeft al beurt → **BK6** (type = Reserve)

---

## 11. Configuratie per Tenant

De volgende zaken zijn **configureerbaar** (niet hardcoded):

| Instelling | Voorbeeld KV Wageningen |
|------------|------------------------|
| Supplier-teams per ronde | Wag 1-5, BK6-8, MW1-3, U19-1/2, J1 |
| KNKV auto-assign teams | Wedstrijden van Wag 1, 2, 3 en U19-1 |
| Bufferaanname wedstrijdduur | 90 minuten |
| Aankomsttijd voor uitwedstrijd | 45 minuten |
| Fallback reistijd (als API faalt) | 45 minuten |
| Voorkeurs-matching regels | Zie §4.2 tabel |

---

## 12. Keuzes & Afwegingen

### Keuze 1: Reistijdberekening — hoe nauwkeurig?

| Optie | Voordelen | Nadelen |
|-------|-----------|---------|
| **A. Route-API (OpenRouteService)** | Gratis (2000 req/dag), echte reistijd, nauwkeurig | Externe dependency, moet geocoden, API kan falen |
| **B. Route-API (Google Maps)** | Meest nauwkeurig, betrouwbaar, traffic-aware | Kost ~$5/1000 requests, vendor lock-in |
| **C. Hemelsbreed + factor** | Geen externe API nodig, snel, altijd beschikbaar | Minder nauwkeurig (bochtige wegen, bruggen) |
| **D. Handmatige tabel** | Simpel, geen API, coördinator bepaalt | Onderhoud, niet schaalbaar naar andere clubs |

**Aanbeveling:** Optie A (OpenRouteService) met fallback naar C (hemelsbreed × 1.3). Gratis, nauwkeurig genoeg, en als API faalt heb je altijd een redelijk alternatief. Cache resultaten → na eerste ronde heb je ~0 API calls nodig.

**Te nemen beslissing:** Accepteren we een externe dependency voor nauwkeurigere planning?

---

### Keuze 2: Engine-algoritme — greedy vs optimalisatie

| Optie | Voordelen | Nadelen |
|-------|-----------|---------|
| **A. Greedy (simpel)** | Snel te bouwen, makkelijk te debuggen, < 1 sec | Kan suboptimaal zijn als vroege toewijzing latere blokkeert |
| **B. Greedy + backtracking** | Beter resultaat bij conflicten, nog steeds snel | Iets complexer, edge cases |
| **C. Constraint solver (OR-Tools)** | Mathematisch optimaal, schaalt naar grote clubs | Overkill voor ~50 wedstrijden, complexe setup, moeilijker te debuggen |
| **D. Meerdere greedy runs + beste kiezen** | Simpel maar beter dan single greedy | Iets langere runtime, maar nog steeds < 1 sec |

**Aanbeveling:** Optie D. Meerdere runs met random volgorde, pak de verdeling met kleinste verschil. Simpel te bouwen, beter resultaat dan single greedy, en de coördinator kan altijd handmatig bijsturen.

**Te nemen beslissing:** Is "bijna optimaal + handmatig bijsturen" acceptabel, of moet het wiskundig perfect zijn?

---

### Keuze 3: Hoe krijgen we namen terug van teams?

| Optie | Voordelen | Nadelen |
|-------|-----------|---------|
| **A. Export PDF/Excel → mail → handmatig terugvoeren** | Simpelste MVP, past bij huidig proces | Geen tijdwinst bij stap 5-6 van het proces |
| **B. Self-service portal (teambegeleider logt in)** | Geen mail-heen-en-weer, direct in systeem | Moet gebouwd, accounts beheren, adoptie-risico |
| **C. Gedeelde link (geen login)** | Laagdrempelig, team vult namen in via link | Minder veilig, geen audit trail |
| **D. WhatsApp-bot / e-mail reply parsing** | Past bij communicatiegedrag teams | Complex, foutgevoelig, AI nodig |

**Aanbeveling:** Start met A (export), bouw naar B of C toe. De €30/maand waarde zit in het *genereren* van de verdeling, niet in het terugkrijgen van namen. Dat is nu al een werkend proces.

**Te nemen beslissing:** Is de MVP puur de verdelings-engine + export, of willen we direct een portal?

---

### Keuze 4: Reserve-notificatie — automatisch of handmatig?

| Optie | Voordelen | Nadelen |
|-------|-----------|---------|
| **A. Handmatig (coördinator bekijkt overzicht, stuurt zelf bericht)** | Simpel, geen e-mail infra nodig, coördinator houdt controle | Kost nog steeds handmatig werk op donderdag |
| **B. Automatische e-mail op donderdag** | Volledig geautomatiseerd, 0 werk voor coördinator | E-mail setup, spam-risico, wat als sync fout is? |
| **C. Donderdag-overzicht in app + 1-klik "stuur notificatie"** | Coördinator bevestigt, maar app stuurt | Beste van beide, iets meer te bouwen |

**Aanbeveling:** Optie A voor MVP, optie C als snelle follow-up. De nightly sync detecteert al of KNKV een scheids heeft ingevuld — het donderdag-overzicht is dan 1 API call. De notificatie zelf kan later.

**Te nemen beslissing:** Bouwen we e-mail/notificatie-infra in de MVP?

---

### Keuze 5: Matching-voorkeuren — hardcoded of configureerbaar?

| Optie | Voordelen | Nadelen |
|-------|-----------|---------|
| **A. Hardcoded regels (3 voorkeursregels in code)** | Snel te bouwen, geen UI nodig, past bij KV Wageningen | Niet flexibel voor andere clubs |
| **B. Configureerbare matrix (admin UI)** | Flexibel, multi-tenant ready | Complexe UI, meer entities, overengineered voor 1 club |
| **C. Hardcoded met feature flag voor override** | Snel nu, uitbreidbaar later | Iets meer code, maar goede middenweg |

**Aanbeveling:** Optie A voor MVP. De 3 regels (§4.2) zijn specifiek voor korfbal en waarschijnlijk vergelijkbaar bij andere korfbalclubs. Als er een 2e club komt met afwijkende regels, refactoren we naar B.

**Te nemen beslissing:** Bouwen we voor 1 club of meteen multi-tenant flexible matching?

---

### Keuze 6: Wanneer draait de engine?

| Optie | Voordelen | Nadelen |
|-------|-----------|---------|
| **A. On-demand (coördinator klikt "Genereer")** | Volledige controle, geen verrassingen | Coördinator moet eraan denken |
| **B. Automatisch bij nieuwe sync-data** | Altijd up-to-date | Kan bestaande handmatige aanpassingen overschrijven |
| **C. Suggestie-modus (engine draait, toont diff, coördinator bevestigt)** | Smart + controle | Meer UI-werk |

**Aanbeveling:** Optie A. De coördinator maakt bewust een ronde aan en genereert. Dit past bij het 4x per seizoen ritme. Automatisch hergenereren zou handmatige swaps overschrijven.

**Te nemen beslissing:** Is de engine een "tool" (coördinator beslist wanneer) of een "autopilot"?

---

### Keuze 7: Eerlijke verdeling — per ronde of cumulatief over seizoen?

| Optie | Voordelen | Nadelen |
|-------|-----------|---------|
| **A. Per ronde gelijk** | Simpel, duidelijk, past bij Lammert's beschrijving | Als team in ronde 1 meer had, wordt dat niet gecompenseerd in ronde 2 |
| **B. Cumulatief over seizoen** | Eerlijkst over hele jaar | Complexer, en als team-samenstelling wisselt per ronde wordt het raar |
| **C. Per ronde gelijk + seizoenstotaal als tiebreaker** | Best of both worlds | Iets meer logica, maar niet complex |

**Aanbeveling:** Optie C. Primair per ronde gelijk verdelen (max verschil 1). Bij gelijke score → team met minste seizoenstotaal krijgt voorrang. Eenvoudig en eerlijk.

**Te nemen beslissing:** Moet Lammert hier nog input op geven, of is "per ronde gelijk + seizoen als tiebreaker" logisch?

---

### Keuze 8: Hoe gaan we om met afgelaste wedstrijden na toewijzing?

| Optie | Voordelen | Nadelen |
|-------|-----------|---------|
| **A. Assignment verwijderen, teller -1** | Eerlijk: team krijgt "beurt terug" | Kan verdeling scheef trekken als 1 team veel afgelastingen heeft |
| **B. Assignment markeren als afgelast, teller behouden** | Simpel, geen herverdeling nodig | Oneerlijk: team "verliest" een beurt zonder te fluiten |
| **C. Assignment afgelast + automatisch herverdelen** | Meest eerlijk | Complexer, kan cascade veroorzaken |

**Aanbeveling:** Optie A. Bij afgelasting gaat de beurt niet mee in de teller. Als de wedstrijd later opnieuw wordt gepland (inhaalwedstrijd), komt hij weer in de pool voor de volgende engine-run.

**Te nemen beslissing:** Telt een afgelaste wedstrijd als "beurt gehad"?

---

### Keuze 9: Hosting & kosten — past het in het €30/maand model?

| Optie | Voordelen | Nadelen |
|-------|-----------|---------|
| **A. Azure (App Service Free/Basic + SQL Basic)** | Schaalbaar, past bij stack, ~€15-25/maand | Azure kennis nodig, kan duurder worden |
| **B. Shared hosting (Railway/Render)** | Goedkoop (~€5-10/maand), simpele deploy | Minder controle, .NET support varieert |
| **C. VPS (Hetzner/DigitalOcean)** | Goedkoopst (~€5/maand), volledige controle | Zelf beheren, updates, backups |
| **D. Azure Container Apps (consumption)** | Pay-per-use, kan €0 zijn bij laag gebruik | Cold starts, complexity |

**Aanbeveling:** Optie A (Azure App Service B1 + Azure SQL Basic) of optie D (Container Apps) als het gebruik laag is. Met 1 club en 4x per seizoen engine-runs is het gebruik minimaal. Hosting < €15/maand is realistisch.

**Te nemen beslissing:** Waar hosten we, en is er budget voor de route-API (of blijven we bij gratis)?

---

### Keuze 10: Hoe detecteren we welke wedstrijden "van ons" zijn?

| Optie | Voordelen | Nadelen |
|-------|-----------|---------|
| **A. Thuiswedstrijd + scheidsrechters = leeg** | Simpel, data-driven, werkt nu al | Mist KNKV-auto-assign wedstrijden die later gevuld worden |
| **B. A + KNKV-auto-assign lijst (configureerbaar)** | Nauwkeuriger, geen onnodige reserves | Moet geconfigureerd worden, kan veranderen |
| **C. A + B + nightly sync detectie (was leeg, nu gevuld = KNKV deed het)** | Meest compleet, leert over tijd | Iets meer logica in sync |

**Aanbeveling:** Optie C. Sync houdt bij wanneer een scheidsrechter verschijnt in Sportlink. Als wij geen assignment hadden → KNKV deed het → die wedstrijd is nooit van ons. Dit bouwt vanzelf een "KNKV-patroon" op.

**Te nemen beslissing:** Willen we dit patroon automatisch leren, of is handmatige config (Wag 1-3, U19-1) voldoende?

---

### Overzicht: Wat moet nu besloten worden vs. later?

| Keuze | Wanneer beslissen? | Impact op MVP |
|-------|-------------------|---------------|
| 1. Reistijd-API | **Nu** (bepaalt of we externe dependency hebben) | Hoog |
| 2. Engine-algoritme | **Nu** (kern van de app) | Hoog |
| 3. Namen terugkrijgen | Later (export is genoeg voor MVP) | Laag |
| 4. Reserve-notificatie | Later (handmatig is OK voor start) | Laag |
| 5. Matching configureerbaar | Later (hardcoded is OK voor 1 club) | Laag |
| 6. Wanneer engine draait | **Nu** (beïnvloedt UX-flow) | Midden |
| 7. Verdeling per ronde/seizoen | **Nu** (beïnvloedt engine-logica) | Midden |
| 8. Afgelaste wedstrijden | **Nu** (beïnvloedt teller-logica) | Midden |
| 9. Hosting | Later (eerst lokaal werkend) | Laag |
| 10. Wedstrijd-detectie | **Nu** (beïnvloedt welke wedstrijden de engine krijgt) | Hoog |

---

## Appendix A: Antwoorden coördinator (mei 2026)

<details>
<summary>Klik om originele antwoorden te tonen</summary>

**Vraag 1 (Senioren 1-5 supplier?):**
Alle seniorenteams moeten fluiten, inclusief Wageningen 1. Daarnaast ook de oudste jeugdteams (U19-1, U19-2, J1).

**Vraag 2 (BK-teams actief?):**
Dit zijn breedtekorfbalteams. Aantal is variabel per competitieperiode. Per jaar drie competities: najaar-veld / zaalcompetitie / voorjaar-veld. Aantal wordt per periode opnieuw bepaald.

**Vraag 3 (Minimum leeftijd fluiten):**
Oudste jeugdteams: U19-1, U19-2 (wedstrijd) en J1 (breedte). A-teams fluiten NIET.

**Vraag 4 (Matching-matrix):**
In principe alle teams, maar wedstrijdkorfbalteams worden vaker ingezet voor wedstrijdkorfbal. B4/jeugdwedstrijden komen vaker bij BK-teams en jeugdteams. Reserve-klasse wedstrijden → mensen die op hoger niveau spelen (Wag 1-5).

**Vraag 5 (Reistijd):**
Aanreistijd afhankelijk van waar het team speelt + circa driekwartier voor start aanwezig zijn.

**Vraag 6 (MW3):**
Drie midweekteams bevestigd. MW-teams spelen doordeweeks (di/wo avond). BK-teams worden vaak bezet door mensen die aan het afbouwen zijn. Fluiten MW-wedstrijden is minder geschikt voor jeugd/BK.

**Vraag 7 (Ereklasse zonder scheids):**
Wageningen 1-3 en U19-1 krijgen altijd KNKV-scheidsrechter. Geen eigen planning hiervoor.

**Vraag 8 (Proces teamindeling):**
Lijstjes per team naar teambegeleiders, 4x per jaar (voor elke competitieperiode). Binnen een week retour met namen. Teams zijn verantwoordelijk. BK-wedstrijden → namen in Sportlink. Wedstrijdkorfbal → reservelijst (geen Sportlink-toegang). Donderdag vóór speelweekend → reserves horen of ze moeten fluiten.

**Vraag 9 (Alleen thuiswedstrijden):**
Alleen verantwoordelijk voor thuiswedstrijden.

**Vraag 10 (Wie fluit niet):**
Alleen de genoemde supplier-teams fluiten. Overige teams niet.

**Vraag 11 (Hoeveel wedstrijden per team):**
Per periode gelijkelijk verdelen over alle supplier-teams. Geen maximum per speelweekend. Binnen elke periode ≈ evenveel per team.



## Vraag 12 (Hoe omgaan met afgelastingen):**Afgelaste wedstrijden worden verwijderd uit de planning, en de beurt gaat terug in de teller.

## Vraag 13 Hoe zit het met bondswedstrijden? Moeten wij bonds scheidsrechters aanleveren?

</details>
