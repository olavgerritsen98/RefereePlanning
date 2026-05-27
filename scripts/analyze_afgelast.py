import json

with open('afgelast_temp.json') as f:
    data = json.load(f)

print(f'Aantal afgelastingen: {len(data)}')
if data:
    print('Keys:', list(data[0].keys()))
    for m in data[:3]:
        print(f"{m.get('wedstrijddatum','')} | {m.get('thuisteam','')} vs {m.get('uitteam','')} | reden: {m.get('reden','')}")
