SELECT 
    a.Id,
    a.Nome,
    a.Email,
    a.Matricula,
    a.Periodo,
    (SELECT AVG(Valor) FROM Notas WHERE AlunoId = a.Id) as MediaGeral
FROM Alunos a;