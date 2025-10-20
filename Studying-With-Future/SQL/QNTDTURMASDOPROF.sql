SELECT 
    u.Id,
    u.Nome,
    u.Formacao,
    u.Especialidade,
    COUNT(t.Id) AS QuantidadeTurmas
FROM Usuarios u
LEFT JOIN Turmas t ON u.Id = t.ProfessorId
WHERE u.TipoUsuario = 'Professor'
GROUP BY u.Id, u.Nome, u.Formacao, u.Especialidade;
