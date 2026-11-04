# Narzędzia PolSkrypt

Wersja dokumentu: 0.1.0

## Spis treści

1. [Analiza statyczna (resolver)](#1-analiza-statyczna-resolver)
2. [Tryb promptu (REPL)](#2-tryb-promptu-repl)

---

## 1. Analiza statyczna (resolver)

Resolver analizuje drzewo składniowe programu przed jego wykonaniem. Ma dwa zadania:

1. **Wyznaczanie zasięgu zmiennych lokalnych.** Dla każdego użycia zmiennej lokalnej ustala, w którym otaczającym bloku została zadeklarowana. Dzięki temu zasięg jest blokowy, shadowing działa przewidywalnie, a domknięcia widzą zmienne z otaczającego zasięgu.
2. **Wykrywanie błędów.** Część błędów jest zgłaszana od razu, a nie dopiero w chwili, gdy wykonanie dojdzie do wadliwego miejsca.

Resolver działą automatycznie przed każdym uruchomieniem.

### Wykrywane błędy

| Reguła | Komunikat |
|--------|-----------|
| `przerwij` użyte poza pętlą | `Wyrzucono 'przerwij' poza pętlą.` |
| `zwróć` użyte poza funkcją | `Nie można zwrócić wartości spoza funkcji.` |
| Odczyt zmiennej lokalnej w jej własnym inicjatorze | `Nie można odczytać lokalnej zmiennej w jej własnym inicjatorze.` |
| Ponowna deklaracja zmiennej w tym samym zasięgu lokalnym | `Zmienna o tej nazwie już istnieje w tym zakresie.` |

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

#### Odczyt zmiennej w jej własnym inicjatorze

Reguła dotyczy zmiennych **lokalnych** (zadeklarowanych w bloku lub funkcji). Chroni przed przypadkowym odwołaniem się do zmiennej, która jeszcze nie ma wartości, szczególnie gdy przesłania ona zmienną zewnętrzną:

```text
zmienna a = 1;
{
    zmienna a = a + 1;          // błąd: 'a' po prawej stronie to jeszcze nie zainicjowane lokalne 'a'
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

### Zasięg globalny

Resolver nie śledzi zmiennych globalnych. W konsekwencji reguły dotyczące deklaracji (ponowna deklaracja, odczyt w inicjatorze) obowiązują wyłącznie w zasięgach lokalnych, a na poziomie globalnym nie są sprawdzane przed uruchomieniem.

### Czego resolver nie sprawdza

- **Użycia niezadeklarowanej zmiennej.** Nazwa, której nie znaleziono w żadnym zasięgu lokalnym, jest traktowana jako globalna i nie jest tu weryfikowana. Ewentualny błąd zostanie więc zgłoszony dopiero podczas wykonania *(do potwierdzenia)*.
- **Liczby argumentów wywołania funkcji.** Zgodność liczby argumentów jest sprawdzana w czasie działania programu *(do potwierdzenia)*.
- **Typów i wartości.** Typowanie jest dynamiczne, więc np. dzielenie przez 0 (`1 / 0`) czy dodawanie napisu do liczby (`"a" + 1`) są zgłaszane dopiero podczas wykonania.

---

## 2. Tryb promptu (REPL)

Tryb promptu pozwala wpisywać kod linia po linii i od razu widzieć wyniki.

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

### Przypisania

Przypisanie jest wyrażeniem, więc również zwraca wartość. Przypisanie złożone najpierw zmienia zmienną, a następnie zwraca jej nową wartość.

```text
> a += 1
6
> a
6
```

Wyrażenie nie wyświetli się jeżeli zostanie one zakończone znakiem `;`.