# Regelset Scheidsrechtertoewijzing — KV Wageningen

## Status: CONCEPT (te valideren met coördinator)

---

## 1. Wie moet fluiten? (Supplier Teams)

Op basis van context_business.md en Sportlink data:

| Categorie | Teams | Bevestigd? |
|-----------|-------|------------|
| Senioren (wedstrijdsport) | Wageningen 1 t/m 5 | ⚠️ Zie vraag 1 |
| Senioren (breedtekorfbal) | Wageningen 6 t/m 9 | ⚠️ Zie vraag 2 |
| Midweek | Wageningen MW1, MW2 | ✅ Ja (context_business.md) |
| A-jeugd | Wageningen A1 t/m A5 | ⚠️ Zie vraag 3 |
| U19/U17/U15 | Wageningen U19-1/2, U17-1/2, U15-1/2 | ⚠️ Zie vraag 4 |
| Oudste J-jeugd (B-rood) | Wageningen J1, J2, J3 | ⚠️ Zie vraag 5 |

**Wat NIET fluiten (te jong):**
- J4 en lager (B-oranje, B-geel, B4-klassen)
- B, C, D, E, F teams (geen leeftijdscategorie in Sportlink API)

---

## 2. Welke wedstrijden moeten wij plannen?

Uit de Sportlink data blijkt:
- Wedstrijden MET scheidsrechter in Sportlink → KNKV wijst toe, wij hoeven niks te doen
- Wedstrijden ZONDER scheidsrechter → wij moeten een supplier team toewijzen

**Verdeling zonder scheidsrechter (uit echte data):**

| Klasse | Aantal | Type |
|--------|--------|------|
| B4-geel/groen/blauw | 19 | Jongste jeugd |
| B-rood/oranje/geel | 10 | Oudere jeugd |
| U15/U17/U19 | 4 | Wedstrijdsport jeugd |
| Reserve ek/ok/1e/2e | 4 | Lagere senioren |
| Ereklasse | 1 | Hoogste senioren |
| Midweek | 2 | Midweek senioren |
| Senioren BK | 1 | Breedtekorfbal |

---

## 3. Toewijzingsregels (Harde Constraints)

### Regel 1: Niveau/Leeftijd Matching
> "Jeugdteams (als supplier) worden gekoppeld aan de jongste jeugdteams."
> "Wedstrijdsport-teams fluiten de reservebeurten in het lagere wedstrijdsport."

**Wat we weten:**
- Jeugd-suppliers (A-jeugd/J1-J3?) → fluiten B4-wedstrijden (jongste jeugd)
- Senioren-suppliers → fluiten senioren-wedstrijden (reserve klassen, BK, midweek)
- Wedstrijdsport jeugd (U19/U17/U15 als supplier?) → fluiten lagere wedstrijdsport

**⚠️ GAP:** De exacte matching-matrix is onduidelijk. Zie vraag 6.

### Regel 2: Tijdconflict Buffer (1,5 uur)
- Een supplier team mag NIET worden ingepland als ze zelf spelen binnen 1,5 uur (voor of na de te fluiten wedstrijd).
- **Sportlink data beschikbaar:** ✅ Ja — we hebben `wedstrijddatum` + `aanvangstijd` voor zowel thuis- als uitwedstrijden.

### Regel 3: Reistijd bij Uitwedstrijd
- Als een supplier team een uitwedstrijd speelt, moet reistijd worden meegerekend in de buffer.
- **Sportlink data beschikbaar:** ⚠️ Deels — we hebben `accommodatie` en `plaats` maar GEEN coördinaten/afstand.

**⚠️ GAP:** Hoe berekenen we reistijd? Zie vraag 7.

---

## 4. Toewijzingsregels (Zachte Constraints)

### Regel 4: Eerlijke Verdeling
- Alle supplier-teams krijgen gedurende het seizoen ~evenveel fluitbeurten.
- **Implementatie:** Teller bijhouden per supplier team, team met minste beurten krijgt voorkeur.

---

## 5. Wat Sportlink API WEL levert

| Informatie | Endpoint | Veld | Status |
|------------|----------|------|--------|
| Thuiswedstrijden + tijden | `programma?thuis=ja` | `wedstrijddatum`, `aanvangstijd` | ✅ |
| Uitwedstrijden + tijden | `programma?uit=ja` | `wedstrijddatum`, `aanvangstijd` | ✅ |
| Team naam & klasse | `teams` | `teamnaam`, `klasse`, `leeftijdscategorie` | ✅ |
| Of er al een scheidsrechter is | `programma` | `scheidsrechters` | ✅ |
| Locatie uitwedstrijd | `programma?uit=ja` | `accommodatie`, `plaats` | ✅ |
| Competitiesoort | `teams` | `spelsoort`, `competitiesoort` | ✅ |
| Afgelastingen | `afgelastingen` | - | ✅ (leeg = geen afgelast) |
| Wedstrijdcode (uniek ID) | `programma` | `wedstrijdcode` | ✅ |

## 6. Wat Sportlink API NIET levert (gaps)

| Informatie | Impact | Oplossing |
|------------|--------|-----------|
| Coördinaten/afstand | Reistijd niet berekenbaar | Externe API of handmatige tabel |
| Wedstrijdduur | Buffer berekening | Aannemen: 1 uur + 30 min uitloop = 1,5 uur |
| Welke teams supplier zijn | Kern van de logica | Configuratie per tenant |
| Seizoensplanning (alle weken) | Historische beurten tellen | Ophalen met groot `aantaldagen` |

---

## 7. OPEN VRAGEN (gaps die jij moet beantwoorden)

### Vraag 1: Fluiten Senioren 1 t/m 5 ook?
Wageningen 1 speelt Ereklasse. Moeten zij ook fluiten, of zijn alleen de lagere teams (2-5 = reserve klassen) supplier? Of beginnen suppliers pas bij team 2?

### Vraag 2: Senioren 6 t/m 9 (BK/geen klasse)
Teams 6-9 staan als "Senioren BK" of hebben géén klasse in Sportlink. Zijn dit actieve teams die moeten fluiten? Of zijn het "papieren" teams?

### Vraag 3: A-jeugd — allemaal supplier?
Er zijn 5 A-teams (A1-A5). Zijn ze ALLEMAAL supplier, of alleen de oudste (A1/A2)? De A-teams hebben `leeftijdscategorie: "Jeugd"` maar geen duidelijk niveau-onderscheid in Sportlink.

### Vraag 4: U19/U17/U15 — supplier of niet?
Deze teams spelen wedstrijdsport (competitie). Moeten zij ook fluiten? Of fluiten zij ALLEEN lagere wedstrijdsport-wedstrijden van hun eigen niveau?

### Vraag 5: J1/J2/J3 — supplier of niet?
J1-J3 zitten in de B-rood klasse (hoogste jeugd). Zijn dit suppliers die jongere jeugd fluiten? Of zijn ze te jong?

### Vraag 6: Matching-matrix — wie fluit wat?
als Wageningen J25 (B4-blauw, jongste jeugd) thuis speelt, welke teams komen dan in aanmerking om te fluiten? En wie fluit een Reserve Overgangsklasse wedstrijd?

### Vraag 7: Reistijd — hoe berekenen?
We hebben de locatie van de uitwedstrijd, maar hoe berekenen we reistijd? Moeten we een externe API (Google Maps) integreren? Of kunnen we volstaan met een handmatige tabel van reistijden tussen veelvoorkomende locaties?

### Vraag 8: MW3 — bestaat die?
In de uitwedstrijden zie ik "Wageningen MW3" maar die staat niet in de teams-lijst. Is dit een nieuw team of een fout?

### Vraag 9: Ereklasse zonder scheidsrechter — bug of feature?
Er staat 1 Ereklasse-wedstrijd zonder scheidsrechter. Wijst KNKV die altijd later toe, of moeten wij daar ook een backup voor plannen?

### Vraag 10: Team indelen
Als er een team is ingedeeld, hoe gaat dan het process van het invullen van een teamlid worden? Wordt dit handmatig gedaan in Sportlink? Krijgt een team een deadline om iemand aan te wijzen? En wat als ze dat niet doen?


### Vraag 11: Uit wedstrijden
Moeten we ook rekening houden met uit wedstrijden of is dat volledig de verantwoordelijkheid van de andere club?
---

## 8. Samenvatting: Wat kunnen we NU al bouwen?

✅ **Duidelijk genoeg om te implementeren:**
- Sportlink data importeren (wedstrijden + teams)
- Tijdconflict-check (1,5 uur buffer)
- Eerlijke verdeling (teller per team)
- Detectie "heeft al scheidsrechter" vs "moet gepland worden"
- Wedstrijden markeren als afgelast

⚠️ **Pas implementeerbaar NA antwoorden op bovenstaande vragen:**
- Welke teams zijn supplier (configuratie)
- Niveau-matching logica
- Reistijdberekening
