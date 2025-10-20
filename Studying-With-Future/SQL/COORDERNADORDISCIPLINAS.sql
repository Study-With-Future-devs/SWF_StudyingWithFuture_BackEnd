SELECT 
    u.Id,
    u.Nome,
    u.AreaCoordenacao,
    d.Nome AS Disciplina
FROM Usuarios u
INNER JOIN Disciplinas d ON u.DisciplinaId = d.Id
WHERE u.TipoUsuario = 'Coordenador';
