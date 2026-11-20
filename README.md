# PolSkrypt
![MAIN](https://github.com/alanpawlukiewicz/PolSkrypt/actions/workflows/main.yml/badge.svg) <br>
Język skryptowy z polskimi słowami kluczowymi, bazowany na książce [Crafting Interpreters](https://craftinginterpreters.com/).

## Możliwości

- typowanie dynamiczne: liczby, napisy, wartości logiczne, listy, funkcje, klasy
- funkcje, funkcje anonimowe i domknięcia
- klasy z konstruktorami, metodami statycznymi i dziedziczeniem
- listy z metodami natywnymi oraz indeksowanie list i napisów
- operacje na plikach (klasa `Plik`)
- moduły: importowanie kodu z innych plików (`importuj`)
- analiza statyczna przed uruchomieniem i komunikaty błędów z numerem linii i nazwą pliku
- tryb interaktywny (REPL)

## Dokumentacja

- [Referencja języka](docs/language-reference.md) – składnia, typy, zmienne, operatory, instrukcje sterujące i funkcje
- [Klasy i dziedziczenie](docs/classes.md) – klasy, metody, konstruktory, dziedziczenie (`dziedziczy_po`), `super` i sprawdzanie klasy obiektu
- [Listy](docs/lists.md) – tworzenie list, indeksowanie i metody natywne list
- [Funkcje wbudowane](docs/builtin-functions.md) – `wypisz`, `wprowadź`, `zegar`, `liczba`, `napis`, `typ`, `jest_instancją`, `klasa_obiektu`
- [Pliki](docs/files.md) – klasa `Plik`: otwieranie, odczyt i zapis plików
- [Moduły](docs/modules.md) – importowanie kodu z innych plików (`importuj`)
- [Narzędzia](docs/tools.md) – analiza statyczna (resolver), tryb promptu (REPL) i komunikaty błędów
- [Changelog](CHANGELOG.md) – historia zmian między wersjami

## Instalacja

Gotowe paczki znajdziesz w zakładce [Releases](../../releases/latest). Pobierz archiwum dla swojego systemu:

| System | Plik |
|---|---|
| Windows (64-bit) | `PolSkrypt-win-x64.zip` |
| Linux (64-bit) | `PolSkrypt-linux-x64.zip` |
| macOS (Apple Silicon: M1/M2/M3/M4) | `PolSkrypt-osx-arm64.zip` |

Aplikacja jest w pełni samodzielna (self-contained), więc nie trzeba instalować .NET.

Po rozpakowaniu można uruchamiać skrypty w następujący sposób:

```
PolSkrypt plik.pols
```

Uruchomienie bez argumentu otwiera tryb interaktywny (REPL):

```
PolSkrypt
```

Aby komenda `PolSkrypt` działała w każdym folderze, dodaj aplikację do zmiennej środowiskowej `PATH`.
