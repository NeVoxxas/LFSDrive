# LFSDrive

## Projekto tikslas

MMO Cruise serveris Live for Speed 0.8C su:
- ekonomika
- garažu
- automobilių parduotuve
- licencijomis
- darbais
- TOP sistema
- banku
- namais
- verslais
- ir t.t.

---

# Architektūra

Core/
    Economy/
    Players/
    UI/
    Vehicles/
    Jobs/

Database/
InSim/

---

# Baigta

## Prisijungimas
- Žaidėjo sukūrimas
- DB įkėlimas
- HUD

## HUD
- Lic
- KM
- Pinigai
- MENU mygtukas

## Menu
- Atidarymas
- Uždarymas
- Background
- Navigation
- Shop Categories
- Shop Vehicles
- Atgal

## Shop

vehicle_shop.json

Palaiko:

- neribotą kategorijų skaičių
- mod automobilius
- kainas
- licencijas

Naudojamas:

CarCode

ne CarName.

Ownership tikrinamas pagal CarCode.

DisplayName naudojamas tik UI.

---

# DB

players

owned_vehicles

car_code

---

# Coding rules

❌ CarName

✅ CarCode

Visa informacija apie automobilius gaunama tik iš VehicleShopService.

Niekur kitur automobilių pavadinimai nelaikomi.

---

# Roadmap

## Shop

- [✅] Puslapiavimas
- [✅] Lic tikrinimas prieš pirkimą
- [✅] Buy dialog

## Garage

- [ ] Mano automobiliai
- [ ] Parduoti

## Profile

- [ ] Statistikos langas

## TOP

- [ ] TOP Money
- [ ] TOP KM
- [ ] TOP Lic

## Economy

- [ ] Bankas
- [ ] Atlyginimai
- [ ] Darbai

## Ateityje

- Namai
- Verslai
- Policija
- Misijos
- Draudimas
- Aukcionas