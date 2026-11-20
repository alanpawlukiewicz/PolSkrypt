# Narzędzia PolSkrypt

Wersja dokumentu: 1.0.0

## Spis treści

1. [Analiza statyczna (resolver)](#1-analiza-statyczna-resolver)
2. [Tryb promptu (REPL)](#2-tryb-promptu-repl)
3. [Komunikaty błędów](#3-komunikaty-błędów)

---

## 1. Analiza statyczna (resolver)

Resolver analizuje drzewo składniowe programu przed jego wykonaniem. Ma dwa zadania:

1. **Wyznaczanie zasięgu zmiennych lokalnych.** Dla każdego użycia zmiennej lokalnej ustala, w którym otaczającym bloku została zadeklarowana. Dzięki temu zasięg jest blokowy, shadowing działa przewidywalnie, a domknięcia widzą zmienne z otaczającego zasięgu. Dotyczy to także zmiennych używanych przez operatory złożone (`+=` itd.) oraz przez indeksowanie list (`lista[i]`, `lista[i] = x`), a także `ja` i `super` w metodach.
2. **Wykrywanie błędów.** Część błędów jest zgłaszana od razu, a nie dopiero w chwili, gdy wykonanie dojdzie do wadliwego miejsca.

Resolver działa automatycznie przed każdym uruchomieniem.

### Wykrywane błędy

| Reguła | Komunikat |
|--------|-----------|
| `przerwij` użyte poza pętlą | `Wyrzucono 'przerwij' poza pętlą.` |
| `zwróć` użyte poza funkcją | `Nie można zwrócić wartości spoza funkcji.` |
| Odczyt zmiennej lokalnej w jej własnym inicjatorze | `Nie można odczytać lokalnej zmiennej w jej własnym inicjatorze.` |
| Ponowna deklaracja zmiennej w tym samym zasięgu lokalnym | `Zmienna o tej nazwie już istnieje w tym zakresie.` |
| `ja` użyte poza klasą | `Nie można użyć 'ja' poza klasą.` |
| `ja` użyte w metodzie statycznej | `Nie można użyć 'ja' w metodzie statycznej.` |
| `super` użyte poza klasą | `Nie można użyć słowa kluczowego 'super' poza blokiem klasy.` |
| `super` użyte w klasie bez klasy bazowej | `Nie można użyć słowa kluczowego 'super' w klasie bez superklasy.` |
| `super` użyte w metodzie statycznej | `Nie można użyć słowa kluczowego 'super' w metodzie statycznej.` |
| Klasa dziedziczy po samej sobie | `Klasa nie może dziedziczyć po samej sobie.` |
| Zwrócenie wartości z konstruktora | `Nie można zwrócić wartości z konstruktora.` |
| Dwie metody o tej samej nazwie w jednej klasie | `Metoda 'nazwa' została już zdefiniowana w tej klasie.` |
| Dwie metody statyczne o tej samej nazwie w jednej klasie | `Metoda statyczna 'nazwa' została już zdefiniowana w tej klasie.` |
| `importuj` poza najwyższym poziomem pliku | `Import jest dozwolony tylko na najwyższym poziomie pliku.` |

#### `przerwij` poza pętlą

`przerwij` jest dozwolone tylko bezpośrednio w ciele pętli. Funkcja zagnieżdżona w pętli nie dziedziczy tego prawa:

```text
przerwij;                       // błąd: poza pętlą

podczas_gdy (prawda) {
    przerwij;                   // poprawnie
}

podczas_gdy (prawda) {
    zmienna f = funkcja () {
        przerwij;               // błąd: to ciało funkcji, nie pętli
    };
}
```

#### `zwróć` poza funkcją

```text
zwróć 1;                        // błąd: poza funkcją

funkcja f() {
    zwróć 1;                    // poprawnie
}
```

W konstruktorze klasy dozwolone jest samo `zwróć;`, ale `zwróć wartość;` jest błędem.

#### Odczyt zmiennej w jej własnym inicjatorze

Reguła dotyczy zmiennych **lokalnych** (zadeklarowanych w bloku lub funkcji). Chroni przed przypadkowym odwołaniem się do zmiennej, która jeszcze nie ma wartości, szczególnie gdy przesłania ona zmienną zewnętrzną. Obejmuje każdy odczyt takiej zmiennej: zwykły, przez indeks listy i przez operator złożony:

```text
zmienna a = 1;
zmienna b = [1, 2];
zmienna c = 1;
{
    zmienna a = a + 1;          // błąd: 'a' po prawej stronie to jeszcze nie zainicjowane lokalne 'a'
    zmienna b = b[0];           // błąd: odczyt przez indeks
    zmienna c = c += 1;         // błąd: odczyt przez operator złożony
}
```

Funkcja anonimowa może odwoływać się do zmiennej, której jest inicjatorem (np. do rekurencji), bo odczyt następuje dopiero przy wywołaniu:

```text
{
    zmienna f = funkcja () { zwróć f; };   // poprawnie
}
```

#### Ponowna deklaracja w tym samym zasięgu

W zasięgu lokalnym nie można dwa razy zadeklarować zmiennej o tej samej nazwie. Dotyczy to także parametrów funkcji, które współdzielą zasięg z jej ciałem:

```text
{
    zmienna x = 1;
    zmienna x = 2;              // błąd
}

funkcja f(a, a) {               // błąd: powtórzony parametr
    zwróć a;
}

funkcja g(a) {
    zmienna a = 1;              // błąd: parametr 'a' jest już w tym zasięgu
}
```

Przesłonięcie zmiennej z **zewnętrznego** zasięgu jest dozwolone (shadowing):

```text
zmienna x = 1;
{
    zmienna x = 2;              // poprawnie
}
```

#### `ja` i `super`

`ja` jest dostępne tylko w konstruktorze i metodach instancji. `super` jest dostępne tylko w klasie, która dziedziczy po innej klasie, także tylko w jej konstruktorze i metodach instancji. Użycie w funkcji zagnieżdżonej lub anonimowej wewnątrz metody jest dozwolone, o ile ta metoda nie jest statyczna:

```text
ja;                                            // błąd: poza klasą

klasa A {
    metoda f() {
        zwróć funkcja () { zwróć ja; };        // poprawnie
    }
    statyczna metoda g() {
        zwróć ja;                              // błąd: metoda statyczna
    }
    metoda h() {
        zwróć super.h();                       // błąd: klasa A nie ma klasy bazowej
    }
}
```

Szczegóły dotyczące klas opisuje [classes.md](classes.md).

#### Duplikaty metod w klasie

Dwie metody o tej samej nazwie w jednej klasie są błędem. Metody zwykłe i statyczne sprawdzane są osobno, więc metoda zwykła i statyczna mogą mieć tę samą nazwę:

```text
klasa A {
    metoda f() { }
    metoda f() { }                 // błąd
}

klasa B {
    metoda f() { }
    statyczna metoda f() { }       // poprawnie
}
```

#### Import poza najwyższym poziomem

Instrukcja `importuj` może wystąpić tylko bezpośrednio w pliku, poza blokami, funkcjami i klasami (zob. [modules.md](modules.md)):

```text
importuj "mat.pols" jako mat;           // poprawnie

funkcja f() {
    importuj "mat.pols" jako m;         // błąd
}
```

### Zasięg globalny

Resolver nie śledzi zmiennych globalnych. Reguły dotyczące deklaracji są sprawdzane w trakcie czasu wykonania.

Każdy importowany moduł jest analizowany przez resolver osobno, w chwili importu.

## 2. Tryb promptu (REPL)

Tryb promptu pozwala wpisywać kod linia po linii i od razu widzieć wyniki. Zmienne, funkcje i klasy zdefiniowane w jednej linii są dostępne w kolejnych.

### Wyświetlanie wartości wyrażeń

W trybie promptu **nie trzeba używać `wypisz`**, aby zobaczyć wartość wyrażenia. Wpisane wyrażenie jest obliczane, a jego wynik wyświetlany.

```text
> zmienna a = 5;
> a
5
> 3 + 4
7
> zmienna b;
> b
WARTOŚĆ PUSTA
```

Każdy nowy prompt zaczyna się od nowej linii.

```text
> zmienna a = 3;
> a;
3
> wypisz(a);
3
>   // pomimo wypisania samej wartości (bez '\n') promt zaczyna się w nowej linii
```

### Przypisania

Przypisanie jest wyrażeniem, więc również zwraca wartość. Przypisanie złożone najpierw zmienia zmienną, a następnie zwraca jej nową wartość. To samo dotyczy przypisania do elementu listy.

```text
> a += 1
6
> a
6
> zmienna l = [1, 2, 3];
> l[0] = 5
5
```

Wyrażenie nie wyświetli się jeżeli zostanie one zakończone znakiem `;`.

### Importowanie w REPL

W trybie promptu można używać `importuj`. Ścieżki są wtedy liczone względem bieżącego katalogu. Moduł zaimportowany raz pozostaje w pamięci do końca sesji, więc ponowny import tego samego pliku nie wczyta jego nowej wersji.

---

## 3. Komunikaty błędów

Błędy są wypisywane na standardowe wyjście błędów w postaci:

```text
[Linia 3] Błąd na 'x': Oczekiwano ';' po deklaracji zmiennej.     // błąd składni
[Linia 7] Błąd: Pamiętaj cholero, nigdy nie dziel przez 0!          // błąd czasu działania
```

### Nazwa pliku

Błąd powstały w kodzie importowanego modułu zawiera dodatkowo nazwę pliku modułu (ścieżkę względem katalogu, z którego uruchomiono interpreter):

```text
[lib\dziel.pols, Linia 3] Błąd: Pamiętaj cholero, nigdy nie dziel przez 0!
```

Dotyczy to błędów składni, błędów resolvera i błędów czasu działania, także wtedy, gdy funkcja modułu zostanie wywołana z innego pliku. Błędy w pliku głównym i w REPL nie zawierają nazwy pliku.

### Błędy funkcji wbudowanych

Błędy zgłaszane przez funkcje i metody wbudowane (np. `liczba`, `jest_instancją`, `Plik.otwórz`, metody list i plików) wskazują linię, w której nastąpiło wywołanie. Jeśli argumenty wywołania zajmują kilka linii, wskazywana jest linia nawiasu zamykającego.

```text
zmienna a = 1;

zmienna b = liczba("abc");    // [Linia 3] Błąd: Argument musi być liczbą.
```

### Kody wyjścia

Przy uruchamianiu pliku interpreter kończy się kodem:

| Kod | Znaczenie |
|-----|-----------|
| `0` | program wykonał się bez błędów |
| `64` | niepoprawne wywołanie interpretera (więcej niż jeden argument) |
| `65` | błąd składni lub błąd wykryty przez resolver (także w importowanym module) |
| `66` | nie znaleziono pliku podanego przy uruchomieniu |
| `70` | błąd czasu działania |