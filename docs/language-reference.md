# Referencja języka PolSkrypt

Wersja dokumentu: 0.2.0

## Spis treści

1. [Struktura leksykalna](#1-struktura-leksykalna)
2. [Typy danych](#2-typy-danych)
3. [Zmienne i zasięg](#3-zmienne-i-zasięg)
4. [Operatory](#4-operatory)
5. [Instrukcje sterujące](#5-instrukcje-sterujące)
6. [Funkcje](#6-funkcje)
7. [Funkcje anonimowe](#7-funkcje-anonimowe)
8. [Funkcje wbudowane](#8-funkcje-wbudowane)

---

## 1. Struktura leksykalna

### Komentarze

```text
// komentarz do końca linii
/* komentarz blokowy */
```

### Identyfikatory

Muszą się zaczynać od litery lub `_`, reszta może się składać z liter, cyfr i znaków `_`.
Wielkość liter ma znaczenie (`x` i `X` to różne zmienne).

### Słowa kluczowe

Nie mogą być używane jako identyfikatory:

`zmienna` `jeżeli` `w_przeciwnym_wypadku` `podczas_gdy` `dla` `prawda` `fałsz` `wartość_pusta` `lub` `oraz` `zwróć` `przerwij`

### Instrukcje i średniki

Każda instrukcja wyrażeniowa oraz deklaracja zmiennej musi kończyć się średnikiem `;`.
Brak średnika jest błędem składni. Język nie wstawia średników automatycznie.

```text
zmienna x = 5;  // poprawnie
wypisz(x);      // poprawnie
zmienna y = 10  // błąd 
```

Średnika **nie** stawia się po:

- bloku `{ ... }`
- definicji funkcji `funkcja ... { ... }`.

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

### Liczba

Wszystkie liczby są typu double, więc `1` i `1.0` to ta sama wartość. Dzielenie przez 0 jest błędem (nie daje nieskończoności).

### Napis

Napisy nie są niejawnie zamieniane na liczby ani odwrotnie. Operator `+` działa wyłącznie na liczbach. Do łączenia napisów służy operator `.`, który przekształca liczby na napisy.

```text
wypisz(1 + 2);        // wypisze: 3
wypisz("a" . 1);      // wypisze: a1
wypisz("a" + 1);      // błąd
```

### Pusty

`wartość_pusta` jest jedyną wartością swojego typu. Zwraca ją funkcja, w której nie wykonano `zwróć`. Porównanie `wartość_pusta == wartość_pusta` daje `prawda`. Użycie jej w operacji arytmetycznej jest błędem (`wartość_pusta + 1`).

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
- Ponowna deklaracja w tym samym bloku spowoduje nadpisanie wartości wcześniej zainicjowanej zmiennej.
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
```

---

## 4. Operatory

### Priorytety (od najwyższego)

| Typ | Operatory | Łączność |
|------|-----------|----------|
| Wyrażenie | `()` | lewostronna |
| Potęgowy | `**` | prawostronna |
| Jednoargumentowy | `-` `!` | prawostronna |
| Multiplikatywny | `*` `/` `%` | lewostronna |
| Dodatek | `+` `-` `.` | lewostronna |
| Relacyjne | `<` `<=` `>` `>=` | lewostronna |
| Równość | `==` `!=` | lewostronna |
| Logiczne AND | `oraz` | lewostronna |
| Logiczne OR | `lub` | lewostronna |
| Wyrażenie warunkowe | `? :` | prawostronna |
| Proste i złożone przypisanie | `=` `*=` `/=` `%=` `+=` `-=` | prawostronna |
| Przecinek | `,` | lewostronna |

### Operatory arytmetyczne

| Operator | Operandy | Wynik | Uwagi |
|----------|----------|-------|-------|
| `+` | liczba, liczba | liczba | |
| `+` | napis, napis | *błąd* | |
| `+` | napis, liczba | *błąd* | |
| `+` | liczba, napis | *błąd* | |
| `.` | napis, napis | napis | konkatenacja |
| `.` | napis, liczba | napis | konkatenacja z przekształconą liczbą na ciąg znaków |
| `.` | liczba, napis | napis | konkatenacja z przekształconą liczbą na ciąg znaków |
| `.` | liczba, liczba | napis | konkatenacja z przekształconymi liczbami na ciągi znaków |
| `/` | liczba, liczba | liczba | dzielenie przez 0 powoduje błąd |
| `*` | liczba, liczba | liczba | |
| `-` | liczba, liczba | liczba | |
| `%` | liczba, liczba | liczba | |
| `**` | liczba, liczba | liczba | |

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

Dostępne wyłącznie wewnątrz pętli; użycie poza nią spowoduje błąd.

---

## 6. Funkcje

### Definicja i wywołanie

```text
funkcja dodaj(a, b) {
    zwróć a + b;
}

wypisz(dodaj(2, 3));   // wypisze: 5
```

## 7. Funkcje anonimowe

### Definicja i wywołanie

```text
zmienna x = funkcja (a, b) {
    zwróc a + b;
};

wypisz(x(1, 2));   // wypisze: 3
```

### Zasady

- Liczba argumentów musi się zgadzać.
- Liczba argumentów musi być mniejsza od 256. 
- Brak `zwróc` zwraca `wartość_pusta`.
- Funkcje są wartościami pierwszej klasy (można je przypisywać do zmiennych).
- Domknięcia: funkcja zagnieżdżona widzi zmienne z otaczającego zasięgu.

---

## 8. Funkcje wbudowane

| Funkcja | Opis | Przykład | Wynik |
|---------|------|----------|-------|
| `wypisz(x, ...)` | wypisuje wartości oddzielone spacją | `print(1, "a");` | `1 a` |
| `wprowadź()` | pozwala na wprowadzenie wartości zmiennej z klawiatury w postaci napisu | `zmienna a = wprowadź();` | |
| `zegar()` | zwraca obecną godzinę UTC w sekundach w postaci liczby | `wypisz(zegar());` | `1793629096` |