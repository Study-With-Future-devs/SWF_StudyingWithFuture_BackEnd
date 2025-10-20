SELECT 
    u.Id,
    u.Nome,
    u.Email,
    u.Matricula,
    u.Periodo
FROM Usuarios u
INNER JOIN AlunoTurmas at ON u.Id = at.AlunoId
WHERE u.TipoUsuario = 'Aluno'
  AND at.TurmaId = [ID_DA_TURMA];
