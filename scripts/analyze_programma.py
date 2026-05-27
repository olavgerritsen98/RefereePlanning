import json

with open('programma_temp.json') as f:
    data = json.load(f)

if data:
    print('Beschikbare velden:', list(data[0].keys()))
    print(f'\nAantal wedstrijden: {len(data)}')
    print()
    for m in data[:10]:
        print(f"{m.get('wedstrijddatum','')} {m.get('aanvangstijd','')} | {m.get('thuisteam',''):<25} vs {m.get('uitteam',''):<25} | {m.get('klasse','')} | scheids: {m.get('scheidsrechters','')}")
else:
    print('Geen data')
