import json

with open('programma_uit_temp.json') as f:
    data = json.load(f)

if data:
    print(f'Aantal uitwedstrijden: {len(data)}')
    print()
    for m in data[:5]:
        print(f"{m.get('wedstrijddatum','')} {m.get('aanvangstijd','')} | {m.get('thuisteam',''):<25} vs {m.get('uitteam',''):<25} | {m.get('accommodatie','')} | {m.get('plaats','')}")
else:
    print('Geen data')
