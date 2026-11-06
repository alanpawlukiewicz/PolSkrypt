# Changelog

Wszystkie istotne zmiany w projekcie są opisywane w tym pliku.

Format oparty na [Keep a Changelog](https://keepachangelog.com/pl/1.1.0/),
a numeracja wersji zgodna z [Semantic Versioning](https://semver.org/lang/pl/).

## [Niewydane]

## [0.3.0] 2026-11-06

### Dodano
- Klasy z metodami, metodami statycznymi, wyrażeniami oraz konstruktorem.
- Słowa kluczowe `klasa`, `metoda`, `statyczna`, `ja`.
- Funkcje natywne `liczba`, `napis`, `typ`.
- Wyświetlanie nazwy i wersji interpretera przy korzystaniu z REPL.

### Zmieniono
- Operator konkatenacji napisów z `.` i `.=` na `&` i `&=`.
- Zmienne globalne teraz zwracają błąd zamiast nadpisywać wartość przy ponownej deklaracji.
- Działanie funkcji natywnej `wypisz` - dodano wsparcie dla znaków *\n*, *\b*, *\r* oraz funkcja teraz nie kończy każdego wypisania znakiem *newline*.

### Naprawiono
- Komentarze wielolinowe `/* */` niedziałające poprawnie.
- Niektóre znaki spoza zakresu 0-9 będące kwalifikowane jako liczby (np. cyfry arabsko-indyjskie).
- Błędy powiązane z brakiem znaku `;` zwracające zawsze linię "0".
- Operator potęgowania `**` wyrzucający błąd w przypadku, gdy potęga posiadała operator unarny (np. 2 ** -1).

### Zdezaprobowano
- Operatory konkatenacji napisów `.` i `.=`.

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
[0.3.0]: https://github.com/alanpawlukiewicz/PolSkrypt/releases/tag/v0.3.0
[0.2.0]: https://github.com/alanpawlukiewicz/PolSkrypt/releases/tag/v0.2.0
[0.1.0]: https://github.com/alanpawlukiewicz/PolSkrypt/releases/tag/v0.1.0