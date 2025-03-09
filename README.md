Początkowo pomyślałem o użyciu delegatu i kolekcji semaforów w C# gdzie co jakiś czas sprawdzamy czy wszystkie semafory dały "zielone światło" i gdy to się stanie robimy invoke delegatu. 
Jednak zadanie musi zostać zrealizowane na dużo wyższym poziomie abstrakcji, najlepiej z użyciem mikroserwisów.
Potrzebny będzie outbox table. W przypadku używania modułu którego "efektem ubocznym" ma być wysłanie maila należy utworzyć żądanie wysłania maila i je zapisać. Następnie wszystkie używane w procesie moduły należy umieścić w kontenerze.
Background worker będzie sprawdzał czy wszystkie moduły zakończyły działanie. 
Jeśli któryś z modułów w kontenerze rzuci wyjątkiem cała transakcja uznawana jest za nieudaną i mail nie zostaje wysłany. 
Gdy background worker stwierdzi zakończenie pracy wszystkich modułów powodzeniem wysłanie maila zostaje wyzwolone
