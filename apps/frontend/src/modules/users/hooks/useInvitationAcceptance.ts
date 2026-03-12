import { useMutation, useQuery } from "@tanstack/react-query";
import { acceptInvitation, getInvitationByToken } from "../api/usersApi";
import type { AcceptInvitationInput } from "../types/users";

export function useInvitationAcceptance(token?: string) {
  const invitationQuery = useQuery({
    queryKey: ["public", "invitation", token],
    queryFn: () => getInvitationByToken(token ?? ""),
    enabled: Boolean(token),
  });

  const acceptInvitationMutation = useMutation({
    mutationFn: (input: AcceptInvitationInput) => acceptInvitation(token ?? "", input),
  });

  return {
    acceptInvitationMutation,
    invitationQuery,
  };
}
