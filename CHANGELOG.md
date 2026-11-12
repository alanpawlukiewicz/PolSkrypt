# Changelog

Wszystkie istotne zmiany w projekcie są opisywane w tym pliku.

Format oparty na [Keep a Changelog](https://keepachangelog.com/pl/1.1.0/),
a numeracja wersji zgodna z [Semantic Versioning](https://semver.org/lang/pl/).

## [Niewydane]

## [0.4.1] 2026-11-12

### Zmieniono
- Można teraz odwoływać się do elementów literałów, wyniku wywołania, wyniku indeksowania oraz pól obiektów np. `a.b[0]`, `[1, 2, 3][0]`, `fn()[0]`, `l[0][0]`.
- REPL teraz zawsze zaczyna nowy prompt od nowej linii.
- Operatory złożonego przypisania mogą operować teraz na elementach listy oraz wyrażeniach instancji np. `l[0] += 2`, `a.b /= 4`.

### Naprawiono
- Funkcja natywna `typ` zwraca teraz dla listy typ `LISTA`.
- Funkcja natywna `napis` zwraca teraz listę w postaci `[...]` np. `[1, 2, 3]`.

## [0.4.0] 2026-11-11

### Dodano
- Dziedziczenie.
- słowa kluczowe `super`, `dziedziczy_po`.
- Listy tworzone poprzez `[]`.
- Funkcje natywne operujące na listach: `dodaj`, `usuń`, `długość`, `zawiera`, `wstaw`, `pusta`, `kopiuj`, `szukaj`, `policz`, `wyczyść`.
- Aliasy dla niektórych słów kluczowych: `zmienna` -> `zm`, `w_przeciwnym_wypadku` -> `wpw`, `podczas_gdy` -> `pgdy`.

### Zmienono
- Funkcja natywna `wypisz` nie wypisuje teraz dodatkowego znaku " " w przypadku podania jednego argumentu.

### Naprawiono
- Operatory złożonego przypisania niepoprawnie identyfikujące zmienne.
- Operatory `/=`, `%=` i `%` nie sprawdzające czy dzielnik to 0.
- Klasy pozwalające na wielokrotną deklarację metod (zwykłych i statycznych) o tej samej nazwie.

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
[0.4.1]: https://github.com/alanpawlukiewicz/PolSkrypt/releases/tag/v0.4.1
[0.4.0]: https://github.com/alanpawlukiewicz/PolSkrypt/releases/tag/v0.4.0
[0.3.0]: https://github.com/alanpawlukiewicz/PolSkrypt/releases/tag/v0.3.0
[0.2.0]: https://github.com/alanpawlukiewicz/PolSkrypt/releases/tag/v0.2.0
[0.1.0]: https://github.com/alanpawlukiewicz/PolSkrypt/releases/tag/v0.1.0