SELECT 
    d.Id,
    d.Nome,
    d.Descricao
FROM Disciplinas d
LEFT JOIN Turmas t ON d.Id = t.DisciplinaId
WHERE t.Id IS NULL;