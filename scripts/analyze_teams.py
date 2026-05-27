import json

with open('teams_temp.json') as f:
    data = json.load(f)

seen = set()
for t in data:
    key = (t['teamnaam'], t.get('leeftijdscategorie',''), t.get('spelsoort',''), t.get('klasse',''))
    if key not in seen:
        seen.add(key)

print(f'Totaal unieke combinaties: {len(seen)}')
print()
print(f'{"Team":<25} | {"Leeftijd":<15} | {"Spelsoort":<15} | Klasse')
print('-'*100)
for key in sorted(seen, key=lambda x: (x[1], x[0])):
    print(f'{key[0]:<25} | {key[1]:<15} | {key[2]:<15} | {key[3]}')
