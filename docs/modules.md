# Moduły i importowanie w PolSkrypt

Wersja dokumentu: 1.0.0

Część referencji języka PolSkrypt (zob. [language-reference.md](language-reference.md)).

## Spis treści

1. [Czym jest moduł](#1-czym-jest-moduł)
2. [Składnia importu](#2-składnia-importu)
3. [Ścieżki](#3-ścieżki)
4. [Zasady działania](#4-zasady-działania)
5. [Nazwy prywatne](#5-nazwy-prywatne)
6. [Błędy](#6-błędy)

---

## 1. Czym jest moduł

Każdy plik `.pols` może być modułem. Moduł ma **własny zasięg globalny**: zmienne, funkcje i klasy zadeklarowane na najwyższym poziomie pliku należą do tego modułu i nie mieszają się z nazwami pliku, który go importuje.

```text
// matematyka.pols
zmienna PI = 3.14159;

funkcja pole_kola(r) {
    zwróć PI * r * r;
}

klasa Punkt {
    konstruktor(x, y) { ja.x = x; ja.y = y; }
}
```

```text
// main.pols
importuj "matematyka.pols" jako mat;

wypisz(mat.pole_kola(2));       // wypisze: 12.56636
zmienna p = mat.Punkt(1, 2);
```

Funkcje wbudowane (`wypisz`, `typ`, `Plik` itd.) są dostępne w każdym module.

---

## 2. Składnia importu

Dostępne są dwie formy.

### Import całego modułu

```text
importuj "ścieżka.pols" jako nazwa;
```

Pod podaną nazwą dostępny jest obiekt modułu. Jego zawartość odczytuje się przez kropkę: `nazwa.funkcja()`, `nazwa.ZMIENNA`, `nazwa.Klasa()`.

```text
importuj "matematyka.pols" jako mat;
wypisz(mat);          // wypisze: <moduł matematyka>
wypisz(typ(mat));     // wypisze: MODUŁ
```

### Import wybranych nazw

```text
importuj nazwa1, nazwa2 znajdujący_się_w "ścieżka.pols";
importuj nazwa1, nazwa2 zsw "ścieżka.pols";     // alias
```

Wybrane nazwy są definiowane bezpośrednio w pliku importującym:

```text
importuj pole_kola, PI zsw "matematyka.pols";
wypisz(pole_kola(1));   // wypisze: 3.14159
```

Słowa kluczowe `importuj`, `znajdujący_się_w`, `zsw` i `jako` są zarezerwowane i nie mogą być nazwami zmiennych.

---

## 3. Ścieżki

Ścieżka w imporcie jest liczona **względem katalogu pliku, który zawiera instrukcję `importuj`**, a nie względem katalogu, z którego uruchomiono interpreter. Dzięki temu moduły mogą importować inne moduły ze swojego otoczenia:

```text
projekt/
├── main.pols           importuj "lib/pomocnik.pols" jako p;
└── lib/
    ├── pomocnik.pols   importuj "napisy.pols" jako n;   // lib/napisy.pols
    └── napisy.pols
```

Można używać `..`, np. `importuj "../wspolne.pols" jako w;`. W trybie promptu (REPL) ścieżki są liczone od bieżącego katalogu.

---

## 4. Zasady działania

### Moduł wykonuje się raz

Przy pierwszym imporcie plik modułu jest wykonywany w całości, a wynik zapamiętywany. Każdy kolejny import tego samego pliku (z dowolnego miejsca programu, także przez inną ścieżkę względną) zwraca **ten sam** moduł bez ponownego wykonania. Wszystkie miejsca importu dzielą więc stan modułu:

```text
// licznik.pols
zmienna stan = 0;
funkcja zwieksz() { stan += 1; zwróć stan; }
```

```text
importuj "licznik.pols" jako a;
importuj "licznik.pols" jako b;
a.zwieksz();
wypisz(b.zwieksz());   // wypisze: 2
```

### Funkcje modułu widzą zmienne swojego modułu

Funkcja zdefiniowana w module zawsze korzysta ze zmiennych globalnych **swojego** modułu, nawet gdy jest wywoływana z innego pliku, w którym istnieje zmienna o tej samej nazwie. Moduł nie widzi zmiennych pliku, który go importuje.

```text
zmienna PI = 999;
importuj pole_kola zsw "matematyka.pols";
wypisz(pole_kola(1));   // wypisze: 3.14159 (PI z modułu, nie 999)
```

### Importowane nazwy są kopiami wartości

Import wybranych nazw kopiuje ich **bieżące wartości**. Późniejsza zmiana zmiennej wewnątrz modułu nie zmienia zaimportowanej kopii. Aktualną wartość odczytuje się przez obiekt modułu:

```text
importuj stan, zwieksz zsw "licznik.pols";
importuj "licznik.pols" jako licznik;
zwieksz();
wypisz(stan);            // wypisze: 0 (kopia z chwili importu)
wypisz(licznik.stan);    // wypisze: 1
```

Listy i instancje są kopiowane jako referencje, więc zmiany ich zawartości są widoczne w obu miejscach.

### Import tylko na najwyższym poziomie

Instrukcja `importuj` może wystąpić tylko na najwyższym poziomie pliku, nie w bloku, funkcji, metodzie ani instrukcji `jeżeli`.

### Pola modułu są tylko do odczytu

Do zawartości modułu nie można przypisywać spoza niego (`mat.PI = 3;` jest błędem). Zmienne modułu można zmieniać funkcjami tego modułu.

---

## 5. Nazwy prywatne

Nazwy zaczynające się od znaku `_` są **prywatne**: można ich używać wewnątrz modułu, ale nie można ich zaimportować ani odczytać przez obiekt modułu.

```text
// konfiguracja.pols
zmienna _haslo = "tajne";
funkcja sprawdz(h) { zwróć h == _haslo; }
```

```text
importuj "konfiguracja.pols" jako k;
wypisz(k.sprawdz("tajne"));   // wypisze: PRAWDA
wypisz(k._haslo);             // błąd: '_haslo' jest prywatne w module 'konfiguracja'
```

---

## 6. Błędy

### Komunikaty z nazwą pliku

Błędy powstałe w kodzie modułu wskazują nazwę pliku modułu (względem katalogu uruchomienia) i linię. Dotyczy to także błędu w funkcji modułu wywołanej z innego pliku:

```text
[lib\dziel.pols, Linia 3] Błąd: Pamiętaj cholero, nigdy nie dziel przez 0!
```

Błędy w pliku głównym nadal mają postać `[Linia N]`.

### Lista błędów

| Sytuacja | Komunikat |
|----------|-----------|
| `importuj` w bloku, funkcji lub metodzie | `Import jest dozwolony tylko na najwyższym poziomie pliku.` |
| plik modułu nie istnieje | `Nie znaleziono modułu '…'.` |
| błąd składni w module | komunikat parsera z nazwą pliku modułu oraz `Błąd w module '…'.` |
| moduł importuje (pośrednio) sam siebie | `Cykliczny import pliku '…'.` |
| import nazwy, której moduł nie zawiera | `Moduł 'nazwa' nie zawiera 'x'.` |
| odczyt nazwy prywatnej | `'_x' jest prywatne w module 'nazwa'.` |
| nazwa jest już zdefiniowana w pliku importującym | `Nie można zaimportować 'x': nazwa jest już zdefiniowana w tym pliku.` |
| przypisanie do pola modułu | błąd czasu działania |

Konflikt nazw przerywa wykonanie programu. Dotyczy to także dwukrotnego zaimportowania tej samej nazwy i powtórzenia nazwy na liście importu (`importuj a, a zsw …`).
