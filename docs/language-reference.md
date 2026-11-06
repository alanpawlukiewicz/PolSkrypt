# Referencja języka PolSkrypt

Wersja dokumentu: 0.3.0

## Spis treści

1. [Struktura leksykalna](#1-struktura-leksykalna)
2. [Typy danych](#2-typy-danych)
3. [Zmienne i zasięg](#3-zmienne-i-zasięg)
4. [Operatory](#4-operatory)
5. [Instrukcje sterujące](#5-instrukcje-sterujące)
6. [Funkcje](#6-funkcje)
7. [Funkcje anonimowe](#7-funkcje-anonimowe)
8. [Klasy](#8-klasy)
9. [Funkcje wbudowane](#9-funkcje-wbudowane)

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

`zmienna` `funkcja` `jeżeli` `w_przeciwnym_wypadku` `podczas_gdy` `dla` `prawda` `fałsz` `wartość_pusta` `lub` `oraz` `zwróć` `przerwij` `klasa` `metoda` `statyczna` `ja`

Dodatkowo wewnątrz ciała klasy specjalne znaczenie ma nazwa `konstruktor` (zob. [Klasy](#8-klasy)).

### Instrukcje i średniki

Każda instrukcja wyrażeniowa oraz deklaracja zmiennej musi kończyć się średnikiem `;`.
Brak średnika jest błędem składni (komunikat wskazuje linię, w której go zabrakło). Język nie wstawia średników automatycznie.

```text
zmienna x = 5;  // poprawnie
wypisz(x);      // poprawnie
zmienna y = 10  // błąd
```

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
| Funkcja | `funkcja (a, b) { ... }` | funkcje są wartościami pierwszej klasy: można je przypisywać do zmiennych i przekazywać dalej |
| Klasa | `klasa K { ... }` | klasy są wartościami; wywołanie klasy tworzy instancję |
| Instancja | `K(1, 2)` | obiekt utworzony z klasy; przechowuje pola i udostępnia metody |

### Liczba

Wszystkie liczby są typu double, więc `1` i `1.0` to ta sama wartość. Dzielenie przez 0 jest błędem (nie daje nieskończoności).

W literałach liczbowych dozwolone są wyłącznie cyfry `0-9`; inne znaki Unicode będące cyframi (np. cyfry arabsko-indyjskie) nie są traktowane jako liczby.

### Napis

Napisy nie są niejawnie zamieniane na liczby ani odwrotnie. Operator `+` działa wyłącznie na liczbach. Do łączenia napisów służy operator `&`, który przekształca liczby na napisy.

```text
wypisz(1 + 2);        // wypisze: 3
wypisz("a" & 1);      // wypisze: a1
wypisz("a" + 1);      // błąd
```

> **Uwaga:** operator `.` nie służy już do konkatenacji (zdezaprobowany w 0.3.0 razem z `.=`). Kropka oznacza teraz dostęp do pól i metod (zob. [Klasy](#8-klasy)).

### Pusty

`wartość_pusta` jest jedyną wartością swojego typu. Zwraca ją funkcja (lub metoda), w której nie wykonano `zwróć`. Porównanie `wartość_pusta == wartość_pusta` daje `prawda`. Użycie jej w operacji arytmetycznej jest błędem (np. `wartość_pusta + 1`).

### Porównywanie wartości różnych typów

Operator `==` porównuje zarówno wartości, jak i typy, więc wartości różnych typów nigdy nie są równe:

```text
wypisz(1 == "1");     // wypisze: fałsz
wypisz(0 == fałsz);   // wypisze: fałsz
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
| funkcja | prawda |
| klasa, instancja | prawda |

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
```

### Zasady

- Zmienna musi być zadeklarowana (`zmienna`) przed użyciem.
- Ponowna deklaracja zmiennej o tej samej nazwie w tym samym zasięgu jest **błędem**. Dotyczy to zarówno zmiennych w blokach, jak i zmiennych globalnych (do wersji 0.2.0 zmienne globalne były w takim przypadku nadpisywane). Żeby zmienić wartość istniejącej zmiennej, użyj przypisania: `x = 2;`.
- Błędy tego typu są wykrywane już na etapie statycznej analizy kodu (Resolver), przed uruchomieniem programu.
- Zasięg jest blokowy: zmienna istnieje do końca bloku `{ }`.
- Zmienna wewnętrzna może przesłonić zewnętrzną (shadowing).

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
| Wyrażenie, wywołanie, dostęp do pola | `()` `.` | lewostronna |
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
| `%` | liczba, liczba | liczba | reszta z dzielenia |
| `**` | liczba, liczba | liczba | potęgowanie; wykładnik może mieć operator jednoargumentowy, np. `2 ** -1` daje `0.5` |

### Operatory przypisania

Oprócz zwykłego przypisania `=` dostępne są operatory złożone: `+=`, `-=`, `*=`, `/=`, `%=` oraz `&=` (konkatenacja z przypisaniem). Każdy z nich działa jak odpowiadający mu operator binarny, którego wynik jest zapisywany z powrotem do zmiennej (lub pola).

```text
zmienna x = 5;
x += 3;          // x == 8
x %= 3;          // x == 2

zmienna s = "a";
s &= "b";        // s == "ab"
```

### Operatory logiczne

`oraz` i `lub` używają **skróconego obliczania** (short-circuit) i zwracają
jeden z operandów, a nie zawsze `bool`:

```text
wypisz(1 lub 2);   // wypisze: 1
wypisz(5 oraz 7);  // wypisze: 7
```

### Porównania

- `==` porównuje wartości oraz typy danych.
- `<`, `>`, `>=`, `<=` działają tylko na liczbach.

### Tabela zachowań brzegowych

| Wyrażenie | Wynik |
|-----------|-------|
| `1 / 0` | *błąd* |
| `wartość_pusta == wartość_pusta` | `prawda` |
| `wartość_pusta + 1` | *błąd* |
| `"a" + 1` | *błąd* |
| `"a" & 1` | `"a1"` |
| `2 ** -1` | `0.5` |

---

## 5. Instrukcje sterujące

### jeżeli / w_przeciwnym_wypadku

**Składnia**

```text
jeżeli (warunek) { ... } w_przeciwnym_wypadku jeżeli (warunek) { ... } w_przeciwnym_wypadku { ... }
```

### podczas_gdy

```text
podczas_gdy (warunek) { ... }
```

### dla

```text
dla (inicjalizacja; warunek; zmiana) { ... }
```

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
- Funkcje są wartościami pierwszej klasy (można je przypisywać do zmiennych).
- Domknięcia: funkcja zagnieżdżona widzi zmienne z otaczającego zasięgu.

---

## 8. Klasy

### Składnia

```text
klasa Nazwa {
    konstruktor(parametry) { ciało }

    metoda nazwa(parametry) { ciało }

    statyczna metoda nazwa(parametry) { ciało }
}
```

W ciele klasy mogą występować wyłącznie: konstruktor, metody i metody statyczne. Po definicji klasy ani jej elementów nie stawia się średnika.

### Elementy klasy

| Element | Składnia | Opis |
|---------|----------|------|
| Konstruktor | `konstruktor(a, b) { ... }` | wywoływany przy tworzeniu instancji; **nie** poprzedza go słowo `metoda` |
| Metoda | `metoda nazwa(a) { ... }` | metoda instancji; każda zwykła metoda **musi** zaczynać się od słowa `metoda` |
| Metoda statyczna | `statyczna metoda nazwa(a) { ... }` | metoda wywoływana na samej klasie, a nie na instancji; poprzedzona połączonymi słowami `statyczna metoda` |

### Tworzenie instancji

Klasę wywołuje się jak funkcję. Wywołanie tworzy nową instancję i wykonuje jej konstruktor z podanymi argumentami. Liczba argumentów musi zgadzać się z liczbą parametrów konstruktora.

```text
zmienna a = K(1, 2);
```

### Pola i słowo `ja`

Wewnątrz konstruktora i metod słowo kluczowe `ja` oznacza bieżącą instancję. Pola tworzy się przez przypisanie i odczytuje przez operator `.`:

```text
ja.a = a;               // zapis pola (wewnątrz konstruktora lub metody)
zmienna x = obiekt.a;   // odczyt pola
```

PolSkrypt wspiera dynamiczne dodawanie atrybutów:

```
klasa K {}
zmienna a = K();
a.b = 3;
wypisz(a.b); // wypisze: 3
```

Próba odwołania się do nieistniejącego atrybutu lub metody skończy się błędem:

```
klasa K {}
zmienna a = K();
wypisz(a.b);     // błąd
wypisz(a.c());   // błąd
```

### Wywoływanie metod

- metody instancji wywołuje się na instancji: `a.metoda1();`
- metody statyczne wywołuje się na klasie: `K.st_metoda1();`

### Przykład

```text
klasa K {
    metoda metoda1() {
        wypisz("zwykła metoda");
    }

    statyczna metoda st_metoda1() {
        wypisz("statyczna metoda");
    }

    konstruktor(a, b) {
        ja.a = a;
        ja.b = b;
    }
}

zmienna a = K(1, 2);
wypisz(a.a);          // wypisze: 1
wypisz(a.b);          // wypisze: 2
a.metoda1();          // wypisze: zwykła metoda
K.st_metoda1();       // wypisze: statyczna metoda
```

### Zasady

- Metoda bez `zwróć` zwraca `wartość_pusta`.
- Konstruktor **zawsze** zwraca instancję obiektu którego tworzy.
- `ja` jest dostępne wyłącznie wewnątrz klasy.
- Klasa i instancja są wartościami prawdziwymi (zob. [Prawdziwość wartości](#prawdziwość-wartości)).

---

## 9. Funkcje wbudowane

| Funkcja | Opis | Przykład | Wynik |
|---------|------|----------|-------|
| `wypisz(x, ...)` | wypisuje wartości oddzielone spacją; **nie** dodaje na końcu znaku nowej linii; obsługuje sekwencje specjalne `\n`, `\b`, `\r` | `wypisz(1, "a");` | `1 a` |
| `wprowadź()` | pozwala na wprowadzenie wartości zmiennej z klawiatury w postaci napisu | `zmienna a = wprowadź();` | |
| `zegar()` | zwraca obecną godzinę UTC w sekundach w postaci liczby | `wypisz(zegar());` | `1793629096` |
| `liczba(x)` | zamienia wartość `x` na liczbę; zwraca błąd w przypadku niepoprawnego argumentu | `liczba("42");` | `42` |
| `napis(x)` | zamienia wartość `x` na napis | `napis(42);` | `"42"` |
| `typ(x)` | zwraca typ wartości `x` | `wypisz(typ(5));` | `LICZBA` |
