SELECT 
    c.Id,
    c.Nome,
    c.AreaCoordenacao,
    d.Nome as Disciplina
FROM Coordenadores c
INNER JOIN Disciplinas d ON c.DisciplinaId = d.Id;