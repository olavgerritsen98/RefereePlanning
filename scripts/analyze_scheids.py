import json

with open('programma_temp.json') as f:
    data = json.load(f)

# Haal een groter programma op voor betere analyse
import urllib.request
url = "https://data.sportlink.com/programma?clientId=IcvlRrY2Wp&thuis=ja&uit=nee&aantaldagen=250&aantalregels=500"
with urllib.request.urlopen(url) as resp:
    data = json.loads(resp.read())

print(f"Totaal thuiswedstrijden komende 250 dagen: {len(data)}")
print()

# Categoriseer
met_scheids = [m for m in data if m.get('scheidsrechters','').strip()]
zonder_scheids = [m for m in data if not m.get('scheidsrechters','').strip()]

print(f"Met scheidsrechter (KNKV): {len(met_scheids)}")
print(f"Zonder scheidsrechter (wij moeten plannen): {len(zonder_scheids)}")
print()

# Per klasse: welke hebben geen scheids?
from collections import Counter
klassen_zonder = Counter(m.get('klasse','?') for m in zonder_scheids)
klassen_met = Counter(m.get('klasse','?') for m in met_scheids)

print("=== WEDSTRIJDEN ZONDER SCHEIDSRECHTER (per klasse) ===")
for k, v in klassen_zonder.most_common():
    print(f"  {k:<40} : {v}")

print()
print("=== WEDSTRIJDEN MET SCHEIDSRECHTER (per klasse) ===")
for k, v in klassen_met.most_common():
    print(f"  {k:<40} : {v}")
