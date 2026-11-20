# Changelog

Wszystkie istotne zmiany w projekcie są opisywane w tym pliku.

Format oparty na [Keep a Changelog](https://keepachangelog.com/pl/1.1.0/),
a numeracja wersji zgodna z [Semantic Versioning](https://semver.org/lang/pl/).

## [1.0.0] 2026-11-20

### Dodano
- Moduły i importowanie kodu z innych plików:
  - `importuj "plik.pols" jako nazwa;` importuje cały moduł jako obiekt (`nazwa.funkcja()`),
  - `importuj a, b znajdujący_się_w "plik.pols";` (alias `zsw`) importuje wybrane nazwy.
- Słowa kluczowe `importuj`, `znajdujący_się_w`, `zsw`, `jako`.
- Właściwości modułów:
  - każdy moduł ma własny zasięg globalny i jest wykonywany tylko raz, nawet przy wielokrotnym imporcie;
  - ścieżki są liczone względem pliku, który importuje;
  - nazwy zaczynające się od `_` są prywatne;
  - wykrywane są importy cykliczne.
- Klasa natywna `Plik` z metodami statycznymi `otwórz`, `istnieje`, `usuń`, `przemianuj`.
- Tryby otwarcia pliku `"r"`, `"w"`, `"a"`, `"rw"`.
- Metody otwartego pliku: `czytaj`, `czytaj_linie`, `czytaj_znaki`, `wpisz`, `wpisz_linie`, `zamknij`, `zamknięty`, `ustaw`, `przewiń`, `pozycja`, `rozmiar`, `koniec`, `przytnij`, `opróżnij`.
- Indeksowanie napisów (tylko odczyt), np. `"abc"[1]` zwraca `"b"`.
- Funkcja natywna `jest_instancją(x, K)` sprawdzająca, czy obiekt jest instancją klasy lub klasy po niej dziedziczącej.
- Funkcja natywna `klasa_obiektu(x)` zwracająca klasę obiektu.
- Nazwa pliku w komunikatach błędów powstałych w importowanych modułach, np. `[lib\dziel.pols, Linia 3]`.
- Numer linii w komunikatach błędów zgłaszanych przez funkcje i metody wbudowane (np. `liczba`, metody list i plików).
- Funkcja natywna `typ` zwraca nowe wartości: `FUNKCJA`, `KLASA`, `INSTANCJA`, `MODUŁ`, `PLIK`.
- Czytelne opisy wartości specjalnych: `nieskończoność`, `-nieskończoność`, `nie-liczba`.
- Dokumentacja: [files.md](docs/files.md), [modules.md](docs/modules.md), opis komunikatów błędów i kodów wyjścia w [tools.md](docs/tools.md).

### Zmieniono
- **Funkcja natywna `typ` zwraca zawsze jedną ze stałych nazw typów, niezależnie od nazw funkcji i klas.** Dla funkcji zwraca `FUNKCJA` zamiast np. `<funkcja f>`, dla klasy `KLASA` zamiast `<klasa K>`, a dla instancji `INSTANCJA` zamiast `<instancja klasy K>`.
- **Liczby są zawsze zapisywane i odczytywane z kropką dziesiętną, niezależnie od ustawień językowych systemu.** Dotyczy to `napis`, `wypisz`, operatora `&` i funkcji `liczba`. Wcześniej na polskim systemie `napis(3.5)` zwracało `"3,5"`.
- Funkcja natywna `liczba` przyjmuje tylko liczby i napisy. Przecinek nie jest akceptowany jako separator dziesiętny ani separator tysięcy. Dozwolony jest zapis wykładniczy (np. `"1e3"`).
- Funkcja natywna `wypisz` z wieloma argumentami nie dodaje już spacji po ostatnim argumencie.
- Funkcje wbudowane znajdują się w osobnym zasięgu nadrzędnym. Zmienna globalna o tej samej nazwie co funkcja wbudowana przesłania ją, zamiast powodować błąd ponownej deklaracji.
- Zero ujemne jest wypisywane jako `0` zamiast `-0`.
- Słowa `importuj`, `znajdujący_się_w`, `zsw` i `jako` są teraz zarezerwowane i nie mogą być nazwami zmiennych.

### Naprawiono
- Metody list `szukaj` i `policz` kończące działanie interpretera, gdy lista zawierała `wartość_pusta`.
- Funkcja natywna `liczba` niepotrafiąca odczytać liczby z kropką (np. `liczba("3.5")`) na systemach z polskimi ustawieniami językowymi.
- Funkcja natywna `typ`, funkcje anonimowe i metody list wypisywane jako wewnętrzne nazwy klas interpretera (np. `PolSkrypt.Interpretation.Callables.ListCallable`) zamiast `<funkcja natywna>`, `<funkcja anonimowa>`, `<funkcja listy dodaj>`.
- Błędy funkcji natywnych (np. `liczba("abc")`) wypisywane bez numeru linii.
- Dokumentacja funkcji `wypisz` podająca sekwencję `\b` zamiast obsługiwanej `\t`.

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
[0.4.1]: https://github.com/alanpawlukiewicz/PolSkrypt/releases/tag/v1.0.0
[0.4.1]: https://github.com/alanpawlukiewicz/PolSkrypt/releases/tag/v0.4.1
[0.4.0]: https://github.com/alanpawlukiewicz/PolSkrypt/releases/tag/v0.4.0
[0.3.0]: https://github.com/alanpawlukiewicz/PolSkrypt/releases/tag/v0.3.0
[0.2.0]: https://github.com/alanpawlukiewicz/PolSkrypt/releases/tag/v0.2.0
[0.1.0]: https://github.com/alanpawlukiewicz/PolSkrypt/releases/tag/v0.1.0