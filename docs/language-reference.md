# Referencja języka PolSkrypt

Wersja dokumentu: 1.0.0

Dokumentacja jest podzielona na kilka plików:

| Plik | Zawartość |
|------|-----------|
| `language-reference.md` (ten plik) | podstawy języka: składnia, typy, zmienne, operatory, instrukcje sterujące, funkcje |
| [`classes.md`](classes.md) | klasy, metody, dziedziczenie, `super`, sprawdzanie klasy obiektu |
| [`lists.md`](lists.md) | listy i metody natywne list |
| [`builtin-functions.md`](builtin-functions.md) | funkcje wbudowane (`wypisz`, `liczba`, `typ`, `jest_instancją` itd.) |
| [`files.md`](files.md) | operacje na plikach: klasa `Plik` i metody otwartego pliku |
| [`modules.md`](modules.md) | moduły i importowanie (`importuj`) |
| [`tools.md`](tools.md) | narzędzia: analiza statyczna (resolver), komunikaty błędów i tryb promptu (REPL) |

## Spis treści

1. [Struktura leksykalna](#1-struktura-leksykalna)
2. [Typy danych](#2-typy-danych)
3. [Zmienne i zasięg](#3-zmienne-i-zasięg)
4. [Operatory](#4-operatory)
5. [Instrukcje sterujące](#5-instrukcje-sterujące)
6. [Funkcje](#6-funkcje)
7. [Funkcje anonimowe](#7-funkcje-anonimowe)
8. [Klasy i dziedziczenie](classes.md)
9. [Listy](lists.md)
10. [Funkcje wbudowane](builtin-functions.md)
11. [Pliki](files.md)
12. [Moduły i importowanie](modules.md)

---

## 1. Struktura leksykalna

### Komentarze

```text
// komentarz do końca linii
/* komentarz blokowy,
   może obejmować wiele linii */
```

### Identyfikatory

Muszą się zaczynać od litery lub `_`, reszta może się składać z liter, cyfr i znaków `_`.
Wielkość liter ma znaczenie (`x` i `X` to różne zmienne).

### Słowa kluczowe

Nie mogą być używane jako identyfikatory:

`zmienna` `zm` `funkcja` `jeżeli` `w_przeciwnym_wypadku` `wpw` `podczas_gdy` `pgdy` `dla` `prawda` `fałsz` `wartość_pusta` `lub` `oraz` `zwróć` `przerwij` `klasa` `metoda` `statyczna` `dziedziczy_po` `ja` `super` `importuj` `znajdujący_się_w` `zsw` `jako`

Słowa `importuj`, `znajdujący_się_w`, `zsw` i `jako` są zarezerwowane od wersji 1.0.0 (zob. [Moduły](modules.md)).

Dodatkowo wewnątrz ciała klasy specjalne znaczenie ma nazwa `konstruktor` (zob. [Klasy](classes.md)).

### Aliasy słów kluczowych

Dla najczęściej używanych słów kluczowych istnieją krótsze aliasy. Alias zachowuje się dokładnie tak samo jak pełne słowo kluczowe.

| Słowo kluczowe | Alias |
|----------------|-------|
| `zmienna` | `zm` |
| `w_przeciwnym_wypadku` | `wpw` |
| `podczas_gdy` | `pgdy` |
| `znajdujący_się_w` | `zsw` |

```text
zm i = 0;
pgdy (i < 3) {
    jeżeli (i == 1) {
        wypisz("jeden");
    } wpw {
        wypisz("inna");
    }
    i += 1;
}
```

> **Uwaga:** aliasy są również słowami kluczowymi, więc nie można używać ich jako nazw zmiennych ani funkcji. Dotyczy to także istniejących programów, które mają zmienną o nazwie `zm`, `wpw`, `pgdy` lub `zsw`. Identyfikatory tylko *zaczynające się* od aliasu (np. `zm1`, `wpwx`) są poprawne.

### Instrukcje i średniki

Każda instrukcja wyrażeniowa oraz deklaracja zmiennej musi kończyć się średnikiem `;`.
Brak średnika jest błędem składni (komunikat wskazuje linię, w której go zabrakło). Język nie wstawia średników automatycznie.

```text
zmienna x = 5;  // poprawnie
wypisz(x);      // poprawnie
zmienna y = 10  // błąd
```

Instrukcja `importuj` również kończy się średnikiem.

Średnika **nie** stawia się po:

- bloku `{ ... }`
- definicji funkcji `funkcja ... { ... }`
- definicji klasy `klasa ... { ... }` oraz definicjach metod i konstruktora wewnątrz niej.

```text
jeżeli (x > 0) {
    wypisz(x);
}  // bez średnika

funkcja dodaj(a, b) {
    zwróć a + b;
}  // bez średnika
```

---

## 2. Typy danych

PolSkrypt jest językiem typowanym dynamicznie: typ ma wartość, a nie zmienna. Ta sama zmienna może w różnych momentach przechowywać wartości różnych typów.

```text
zmienna x = 5;       // liczba
x = "pięć";          // teraz napis
```

### Zestawienie typów

| Typ | Literały | Opis |
|-----|----------|------|
| Liczba | `0`, `42`, `-7`, `3.14` | liczba zmiennoprzecinkowa podwójnej precyzji (double, IEEE 754); nie ma osobnego typu całkowitego |
| Napis | `"tekst"` | ciąg znaków bez ograniczenia długości |
| Logiczny | `prawda`, `fałsz` | dwie wartości logiczne |
| Pusty | `wartość_pusta` | oznacza brak wartości |
| Lista | `[]`, `[1, "a", prawda]` | uporządkowany, modyfikowalny ciąg wartości dowolnych typów (zob. [Listy](lists.md)) |
| Funkcja | `funkcja (a, b) { ... }` | funkcje są wartościami pierwszej klasy: można je przypisywać do zmiennych i przekazywać dalej |
| Klasa | `klasa K { ... }` | klasy są wartościami; wywołanie klasy tworzy instancję |
| Instancja | `K(1, 2)` | obiekt utworzony z klasy; przechowuje pola i udostępnia metody |
| Moduł | `importuj "m.pols" jako m;` | zaimportowany plik; jego zawartość odczytuje się przez `.` (zob. [Moduły](modules.md)) |
| Plik | `Plik.otwórz("a.txt", "r")` | otwarty plik (zob. [Pliki](files.md)) |

Nazwę rodzaju wartości zwraca funkcja `typ` (np. `LICZBA`, `FUNKCJA`, `INSTANCJA`), a klasę obiektu sprawdzają `jest_instancją` i `klasa_obiektu` (zob. [Funkcje wbudowane](builtin-functions.md)).

### Liczba

Wszystkie liczby są typu double, więc `1` i `1.0` to ta sama wartość. Dzielenie przez 0 jest błędem (nie daje nieskończoności). Dotyczy to wszystkich operatorów z dzielnikiem: `/`, `%`, `/=` i `%=`.

W literałach liczbowych dozwolone są wyłącznie cyfry `0-9`; inne znaki Unicode będące cyframi (np. cyfry arabsko-indyjskie) nie są traktowane jako liczby.

Separatorem dziesiętnym jest zawsze **kropka**, niezależnie od ustawień językowych systemu. Dotyczy to zarówno literałów w kodzie, jak i zamiany liczby na napis (`wypisz`, `napis`, `&`) i napisu na liczbę (`liczba`). Ten sam program wypisuje więc to samo na każdym komputerze:

```text
wypisz(1 / 4);        // wypisze: 0.25
wypisz("x" & 1.5);    // wypisze: x1.5
liczba("2.5");        // 2.5
liczba("2,5");        // błąd
```

Wartości, które nie są zwykłymi liczbami (np. wynik `10 ** 400`), są wypisywane jako `nieskończoność`, `-nieskończoność` i `nie-liczba`. Zero ujemne jest wypisywane jako `0`.

### Napis

Napisy nie są niejawnie zamieniane na liczby ani odwrotnie. Operator `+` działa wyłącznie na liczbach. Do łączenia napisów służy operator `&`, który przekształca liczby na napisy.

```text
wypisz(1 + 2);        // wypisze: 3
wypisz("a" & 1);      // wypisze: a1
wypisz("a" + 1);      // błąd
```

#### Indeksowanie napisów

Do pojedynczych znaków napisu można się odwoływać przez indeks, tak jak do elementów listy. Wynikiem jest jednoznakowy napis. Indeks zaczyna się od zera.

```text
zmienna s = "żółw";
wypisz(s[0]);              // wypisze: ż
wypisz("kot"[2]);          // wypisze: t
wypisz(("ko" & "t")[1]);   // wypisze: o
```

Napisy są niezmienne, więc przypisanie do znaku (`s[0] = "x";`) jest błędem. Indeks musi być nieujemną liczbą całkowitą mniejszą od długości napisu.

> **Uwaga:** operator `.` nie służy już do konkatenacji (zdezaprobowany w 0.3.0 razem z `.=`). Kropka oznacza teraz dostęp do pól i metod (zob. [Klasy](classes.md)).

### Pusty

`wartość_pusta` jest jedyną wartością swojego typu. Zwraca ją funkcja (lub metoda), w której nie wykonano `zwróć`. Porównanie `wartość_pusta == wartość_pusta` daje `prawda`. Użycie jej w operacji arytmetycznej jest błędem (np. `wartość_pusta + 1`).

### Lista

Lista to modyfikowalny ciąg wartości, tworzony literałem w nawiasach kwadratowych i indeksowany od zera. Listy są przekazywane przez referencję: dwie zmienne mogą wskazywać tę samą listę. Pełny opis, w tym metody natywne, znajduje się w [lists.md](lists.md).

```text
zmienna l = [10, 20, 30];
wypisz(l[1]);         // wypisze: 20
l[1] = 99;
```

### Porównywanie wartości różnych typów

Operator `==` porównuje zarówno wartości, jak i typy, więc wartości różnych typów nigdy nie są równe:

```text
wypisz(1 == "1");     // wypisze: fałsz
wypisz(0 == fałsz);   // wypisze: fałsz
```

Listy (podobnie jak instancje) są porównywane przez referencję, a nie przez zawartość. Równe są tylko dwa odwołania do tej samej listy:

```text
zmienna a = [1];
zmienna b = a;
zmienna c = a.kopiuj(); // kopia zmiennej a
wypisz(a == b);       // wypisze: prawda (ta sama lista)
wypisz(a == c);     // wypisze: fałsz (inna lista o takiej samej zawartości)
```

### Prawdziwość wartości

Wszędzie tam, gdzie język oczekuje warunku, wartość dowolnego typu jest interpretowana jako logiczna. Dotyczy to warunków w `jeżeli`, `podczas_gdy`, `dla`, wyrażenia warunkowego `? :` oraz operatorów `oraz` i `lub`.

Zasady są następujące:

1. `wartość_pusta` jest **fałszem**.
2. Wartość logiczna jest sobą: `prawda` jest prawdą, `fałsz` jest fałszem.
3. **Każda inna wartość jest prawdą.**

| Wartość | Interpretacja |
|---------|---------------|
| `fałsz` | fałsz |
| `wartość_pusta` | fałsz |
| `prawda` | prawda |
| `0` | prawda |
| `-1`, `3.14` | prawda |
| `""` (pusty napis) | prawda |
| `"tekst"` | prawda |
| `[]` (pusta lista) | prawda |
| `[1, 2]` | prawda |
| funkcja | prawda |
| klasa, instancja | prawda |
| moduł, plik | prawda |

Do sprawdzenia, czy lista jest pusta, służy metoda `pusta()` (zob. [lists.md](lists.md)).

#### Wpływ na operatory `oraz` i `lub`

Operatory `oraz` i `lub` zwracają jeden z operandów, a nie zawsze wartość logiczną. Prawdziwość decyduje tylko o tym, który operand zostanie zwrócony:

- `a lub b` zwraca `a`, jeśli `a` jest prawdą, w przeciwnym razie `b`.
- `a oraz b` zwraca `a`, jeśli `a` jest fałszem, w przeciwnym razie `b`.

```text
wypisz(wartość_pusta lub 3);   // wypisze: 3
wypisz(0 lub 3);               // wypisze: 0 (zero jest prawdą)
wypisz(0 oraz 3);              // wypisze: 3
wypisz(fałsz oraz 3);          // wypisze: fałsz
```

---

## 3. Zmienne i zasięg

### Składnia

```text
zmienna nazwa = wyrażenie;
zm nazwa = wyrażenie;        // alias
```

### Zasady

- Zmienna musi być zadeklarowana (`zmienna` lub `zm`) przed użyciem.
- Ponowna deklaracja zmiennej o tej samej nazwie w tym samym zasięgu jest **błędem**. Dotyczy to zarówno zmiennych w blokach, jak i zmiennych globalnych (do wersji 0.2.0 zmienne globalne były w takim przypadku nadpisywane). Żeby zmienić wartość istniejącej zmiennej, użyj przypisania: `x = 2;`.
- Funkcje wbudowane (`wypisz`, `typ`, `Plik` itd.) znajdują się w osobnym zasięgu, nadrzędnym wobec zasięgu globalnego pliku. Zmienna globalna o tej samej nazwie (np. `zmienna typ = 1;`) przesłania funkcję wbudowaną w tym pliku, zamiast powodować błąd ponownej deklaracji.
- Każdy plik ma własny zasięg globalny. Zmienne globalne jednego pliku nie są widoczne w plikach, które on importuje (zob. [Moduły](modules.md)).
- Błędy tego typu w zasięgach lokalnych są wykrywane już na etapie statycznej analizy kodu (Resolver), przed uruchomieniem programu. Resolver nie śledzi zmiennych globalnych (zob. [tools.md](tools.md)).
- Zasięg jest blokowy: zmienna istnieje do końca bloku `{ }`.
- Zmienna wewnętrzna może przesłonić zewnętrzną (shadowing).
- Zmiennej lokalnej nie można odczytać w jej własnym inicjatorze, także przez indeks (`zmienna l = l[0];`) ani przypisanie złożone (`zmienna x = x += 1;`).

### Przykład

```text
zmienna x = 1;
{
    zmienna x = 2;
    wypisz(x);   // wypisze: 2
}
wypisz(x);       // wypisze: 1

zmienna x = 3;   // błąd: x jest już zadeklarowana w tym zasięgu
x = 3;           // poprawnie: przypisanie
```

---

## 4. Operatory

### Priorytety (od najwyższego)

| Typ | Operatory | Łączność |
|------|-----------|----------|
| Wyrażenie, wywołanie, dostęp do pola, indeksowanie listy lub napisu | `()` `.` `[]` | lewostronna |
| Potęgowy | `**` | prawostronna |
| Jednoargumentowy | `-` `!` | prawostronna |
| Multiplikatywny | `*` `/` `%` | lewostronna |
| Dodatek | `+` `-` `&` | lewostronna |
| Relacyjne | `<` `<=` `>` `>=` | lewostronna |
| Równość | `==` `!=` | lewostronna |
| Logiczne AND | `oraz` | lewostronna |
| Logiczne OR | `lub` | lewostronna |
| Wyrażenie warunkowe | `? :` | prawostronna |
| Proste i złożone przypisanie | `=` `*=` `/=` `%=` `+=` `-=` `&=` | prawostronna |
| Przecinek | `,` | lewostronna |

### Operatory arytmetyczne

| Operator | Operandy | Wynik | Uwagi |
|----------|----------|-------|-------|
| `+` | liczba, liczba | liczba | |
| `+` | napis, napis | *błąd* | |
| `+` | napis, liczba | *błąd* | |
| `+` | liczba, napis | *błąd* | |
| `&` | napis, napis | napis | konkatenacja |
| `&` | napis, liczba | napis | konkatenacja z przekształconą liczbą na ciąg znaków |
| `&` | liczba, napis | napis | konkatenacja z przekształconą liczbą na ciąg znaków |
| `&` | liczba, liczba | napis | konkatenacja z przekształconymi liczbami na ciągi znaków |
| `/` | liczba, liczba | liczba | dzielenie przez 0 powoduje błąd |
| `*` | liczba, liczba | liczba | |
| `-` | liczba, liczba | liczba | |
| `%` | liczba, liczba | liczba | reszta z dzielenia (ma znak dzielnej: `-7 % 3` daje `-1`); dzielenie przez 0 powoduje błąd |
| `**` | liczba, liczba | liczba | potęgowanie; wykładnik może mieć operator jednoargumentowy, np. `2 ** -1` daje `0.5` |

### Operatory przypisania

Oprócz zwykłego przypisania `=` dostępne są operatory złożone: `+=`, `-=`, `*=`, `/=`, `%=` oraz `&=` (konkatenacja z przypisaniem). Każdy z nich działa jak odpowiadający mu operator binarny, którego wynik jest zapisywany z powrotem do zmiennej. Operatory `/=` i `%=` również zgłaszają błąd przy dzielniku równym 0.

```text
zmienna x = 5;
x += 3;           // x == 8
x %= 3;           // x == 2

zmienna s = "a";
s &= "b";         // s == "ab"

zmienna a = K();
a.pole = 1;
a.pole += 1;      // a.pole == 2

lista = [1, 2];
lista[0] += 1;    // lista == [2, 2]
```

### Operatory logiczne

`oraz` i `lub` używają **skróconego obliczania** (short-circuit) i zwracają
jeden z operandów, a nie zawsze `bool`:

```text
wypisz(1 lub 2);   // wypisze: 1
wypisz(5 oraz 7);  // wypisze: 7
```

### Porównania

- `==` porównuje wartości oraz typy danych (listy i instancje przez referencję).
- `<`, `>`, `>=`, `<=` działają tylko na liczbach.

### Tabela zachowań brzegowych

| Wyrażenie | Wynik |
|-----------|-------|
| `1 / 0` | *błąd* |
| `5 % 0` | *błąd* |
| `wartość_pusta == wartość_pusta` | `prawda` |
| `wartość_pusta + 1` | *błąd* |
| `"a" + 1` | *błąd* |
| `"a" & 1` | `"a1"` |
| `2 ** -1` | `0.5` |
| `-7 % 3` | `-1` |
| `[1] == [1]` | `fałsz` |
| `1 / 4` | `0.25` (zawsze z kropką) |
| `"abc"[1]` | `"b"` |
| `"abc"[3]` | *błąd* |

---

## 5. Instrukcje sterujące

### jeżeli / w_przeciwnym_wypadku

**Składnia**

```text
jeżeli (warunek) { ... } w_przeciwnym_wypadku jeżeli (warunek) { ... } w_przeciwnym_wypadku { ... }
```

Zamiast `w_przeciwnym_wypadku` można użyć aliasu `wpw`:

```text
jeżeli (warunek) { ... } wpw jeżeli (warunek) { ... } wpw { ... }
```

### podczas_gdy

```text
podczas_gdy (warunek) { ... }
pgdy (warunek) { ... }        // alias
```

### dla

```text
dla (inicjalizacja; warunek; zmiana) { ... }
```

Zmienna zadeklarowana w części inicjalizacji istnieje tylko wewnątrz pętli. Każda z trzech części może być pominięta, np. `dla (;;) { ... }` to pętla nieskończona (przerywana przez `przerwij`).

### przerwij

Dostępne wyłącznie wewnątrz pętli; użycie poza nią jest błędem wykrywanym na etapie statycznej analizy kodu.

---

## 6. Funkcje

### Definicja i wywołanie

```text
funkcja dodaj(a, b) {
    zwróć a + b;
}

wypisz(dodaj(2, 3));   // wypisze: 5
```

### Zasady

- `zwróć` jest dozwolone wyłącznie wewnątrz funkcji (lub metody); użycie poza nią jest błędem wykrywanym na etapie statycznej analizy kodu.
- Funkcja, w której nie wykonano `zwróć`, zwraca `wartość_pusta`.
- Liczba argumentów wywołania musi zgadzać się z liczbą parametrów (w przeciwnym razie błąd w czasie działania programu).

## 7. Funkcje anonimowe

### Definicja i wywołanie

```text
zmienna x = funkcja (a, b) {
    zwróć a + b;
};

wypisz(x(1, 2));   // wypisze: 3
```

### Zasady

- Liczba argumentów musi się zgadzać.
- Liczba argumentów musi być mniejsza od 256.
- Brak `zwróć` zwraca `wartość_pusta`.
- Funkcje są wartościami pierwszej klasy (można je przypisywać do zmiennych, przechowywać w listach i przekazywać jako argumenty).
- Domknięcia: funkcja zagnieżdżona widzi zmienne z otaczającego zasięgu.

```text
funkcja dodajacy(n) {
    zwróć funkcja (x) { zwróć x + n; };
}

zmienna dodaj3 = dodajacy(3);
wypisz(dodaj3(4));   // wypisze: 7
```