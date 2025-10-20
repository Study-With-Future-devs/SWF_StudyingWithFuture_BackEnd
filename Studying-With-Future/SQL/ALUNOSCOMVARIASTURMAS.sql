SELECT 
    u.Id,
    u.Nome,
    u.Matricula,
    COUNT(at.TurmaId) AS QuantidadeTurmas
FROM Usuarios u
INNER JOIN AlunoTurmas at ON u.Id = at.AlunoId
WHERE u.TipoUsuario = 'Aluno'
GROUP BY u.Id, u.Nome, u.Matricula
HAVING COUNT(at.TurmaId) > 1;
