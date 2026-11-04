# Changelog

Wszystkie istotne zmiany w projekcie są opisywane w tym pliku.

Format oparty na [Keep a Changelog](https://keepachangelog.com/pl/1.1.0/),
a numeracja wersji zgodna z [Semantic Versioning](https://semver.org/lang/pl/).

## [Niewydane]

## [0.2.0] 2026-11-04

### Dodano
- Operator reszty dzielenia `%`.
- Operatory złożonego przypisanie `+=`, `-=`, `.=`, `*=`, `/=`, `%=`.
- Operator potęgowania `**`.
- Statyczną analizę kodu, wykrywanie błędów na etapie analizy (Resolver).

### Zmienino
- Funkcjonowanie słowa kluczowego `zwróć`, które teraz zwraca błąd w przypadku wykorzystania go poza blokiem funkcji.
- Przypisywanie zmiennych w blokach, które obecnie nie pozwala na zdefiniowanie tej samej zmiennej więcej niż jeden raz.

### Naprawiono
- Słowo kluczowe `przerwij` zwracające niepoprawny błąd w przypadku wykorzystania poza blokiem pętli.

## [0.1.0] - 2026-11-01

### Dodano
- Pierwsza wersja interpretera: zmienne, arytmetyka, `jeżeli`, `podczas_gdy`, `dla`, funkcje, funkcje anonimowe.
- Funkcje natywne: `zegar`, `wypisz`, `wprowadź`

[Niewydane]: https://github.com/alanpawlukiewicz/PolSkrypt/tree/master
[0.2.0]: https://github.com/alanpawlukiewicz/PolSkrypt/releases/tag/v0.2.0
[0.1.0]: https://github.com/alanpawlukiewicz/PolSkrypt/releases/tag/v0.1.0