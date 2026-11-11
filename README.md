# PolSkrypt
![MAIN](https://github.com/alanpawlukiewicz/PolSkrypt/actions/workflows/main.yml/badge.svg) <br>
Język skryptowy bazowany na książce [Crafting Interpreters](https://craftinginterpreters.com/).

## Dokumentacja

- [Referencja języka](docs/language-reference.md) – składnia, typy, zmienne, operatory, instrukcje sterujące i funkcje
- [Klasy i dziedziczenie](docs/classes.md) – klasy, metody, konstruktory, dziedziczenie (`dziedziczy_po`) i `super`
- [Listy](docs/lists.md) – tworzenie list, indeksowanie i metody natywne list
- [Funkcje wbudowane](docs/builtin-functions.md) – `wypisz`, `wprowadź`, `zegar`, `liczba`, `napis`, `typ`
- [Narzędzia](docs/tools.md) – analiza statyczna (resolver) i tryb promptu (REPL)
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

Aby komenda `PolSkrypt` działała w każdym folderze, dodaj aplikację do zmiennej środowiskowej `PATH`.
